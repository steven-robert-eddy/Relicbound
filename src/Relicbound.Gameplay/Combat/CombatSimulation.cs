using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Spells;

namespace Relicbound.Gameplay.Combat;

/// <remarks>
/// The turn-by-turn orchestrator for a single fight. This is a request/response
/// API rather than the architecture doc's illustrative Run(CombatSetup) ->
/// CombatResult sketch, because an interactive fight needs the player to see
/// declared enemy intent and act turn by turn, not receive one final result.
/// A scripted RunToCompletion helper can still satisfy the doc's original
/// shape later, for a balance harness that plays many fights unattended.
/// </remarks>
public sealed class CombatSimulation
{
    private const int MoveCost = 1;

    private readonly Grid _grid;
    private readonly IReadOnlyList<Entity> _entities;
    private readonly Entity _player;
    private readonly IReadOnlyList<Entity> _enemies;
    private readonly ITurnResourceModel _turnResourceModel;
    private readonly EffectResolver _resolver;
    private readonly IRandomSource _random;
    private readonly TriggerRegistry _triggerRegistry;
    private readonly ComposedSpell _basicAttack;

    public CombatSimulation(CombatSetup setup)
    {
        _grid = new Grid(setup.GridWidth, setup.GridHeight);
        _entities = setup.Entities;
        _player = _entities.Single(e => e.Has<PlayerControlled>());
        _enemies = _entities.Where(e => !e.Has<PlayerControlled>()).ToList();
        _turnResourceModel = new ActionPointModel();
        EventBus = new EventBus();
        Journal = new Journal();
        _triggerRegistry = new TriggerRegistry();
        _resolver = new EffectResolver(EventBus, Journal, _triggerRegistry);
        _random = new SplitMix64RandomSource(setup.RandomSeed);
        _basicAttack = setup.BasicAttack;

        foreach (var entity in _entities)
        {
            if (!entity.Has<TurnResources>())
            {
                entity.Add(new TurnResources(_turnResourceModel.StartingResources(entity)));
            }

            if (!entity.Has<Intent>())
            {
                entity.Add(new Intent());
            }

            var equipment = entity.Get<Equipment>();
            if (equipment is null) { continue; }

            for (var slot = 0; slot < Equipment.SlotCount; slot++)
            {
                var artifact = equipment.Get(slot);
                if (artifact is not null)
                {
                    _triggerRegistry.Register(slot, artifact, entity);
                }
            }
        }

        StartRound();
    }

    public IEventBus EventBus { get; }
    public Journal Journal { get; }
    public TriggerRegistry TriggerRegistry => _triggerRegistry;
    public Grid Grid => _grid;
    public IReadOnlyList<Entity> Entities => _entities;
    public int RoundNumber { get; private set; } = 1;
    public CombatWinner Winner { get; private set; } = CombatWinner.None;
    public bool IsCombatOver => Winner != CombatWinner.None;
    public int PlayerActionPoints => _player.Get<TurnResources>()!.ActionPoints;
    public int PlayerMaxActionPoints => _turnResourceModel.StartingResources(_player);

    public bool RequestMove(EntityId actorId, GridPoint destination)
    {
        if (IsCombatOver) { return false; }

        var actor = FindEntity(actorId);
        if (actor is null) { return false; }

        var position = actor.Get<GridPosition>();
        if (position is null) { return false; }

        if (!_grid.IsInBounds(destination)) { return false; }
        if (position.Point.ManhattanDistance(destination) != 1) { return false; }
        if (IsOccupied(destination)) { return false; }

        var cost = new ActionCost(MoveCost);
        if (!_turnResourceModel.CanAfford(actor, cost)) { return false; }

        _turnResourceModel.Spend(actor, cost);
        MoveTo(actor, destination);

        return true;
    }

    public bool RequestAttack(EntityId actorId, EntityId targetId)
    {
        if (IsCombatOver) { return false; }

        var actor = FindEntity(actorId);
        var target = FindEntity(targetId);
        if (actor is null || target is null) { return false; }

        var actorPosition = actor.Get<GridPosition>();
        var targetPosition = target.Get<GridPosition>();
        if (actorPosition is null || targetPosition is null) { return false; }

        if (actorPosition.Point.ManhattanDistance(targetPosition.Point) != 1) { return false; }

        var cost = new ActionCost(_basicAttack.Cost);
        if (!_turnResourceModel.CanAfford(actor, cost)) { return false; }

        _turnResourceModel.Spend(actor, cost);
        CastBasicAttack(actor, target);

        CheckForCombatEnd();
        return true;
    }

    public void EndPlayerTurn()
    {
        if (IsCombatOver) { return; }

        foreach (var enemy in _enemies)
        {
            if (enemy.Get<Health>() is { IsDefeated: true })
            {
                continue;
            }

            ExecuteIntent(enemy);

            if (IsCombatOver) { return; }
        }

        RoundNumber++;
        StartRound();
    }

    private void StartRound()
    {
        _triggerRegistry.ResetRoundBudgets();

        foreach (var entity in _entities)
        {
            _turnResourceModel.Replenish(entity);

            var statuses = entity.Get<Statuses>();
            if (statuses is null) { continue; }

            // Snapshot: resolving a tick effect can itself apply/expire
            // statuses (e.g. a triggered artifact), which would otherwise
            // mutate Active out from under this loop.
            foreach (var active in statuses.Active.ToList())
            {
                var tickEffect = StatusTickEffects.Create(active.Type, active.Stacks);
                if (tickEffect is null) { continue; }

                _resolver.Resolve(
                    tickEffect,
                    new EffectContext(entity, new[] { entity }, _random, depth: 0, EffectOrigin.Status));
            }

            foreach (var expired in statuses.TickDurations())
            {
                var expiredEvent = new StatusExpiredEvent(entity, expired.Type);
                Journal.Record(expiredEvent);
                EventBus.Publish(expiredEvent);
            }
        }

        CheckForCombatEnd();
        if (IsCombatOver) { return; }

        foreach (var enemy in _enemies)
        {
            if (enemy.Get<Health>() is { IsDefeated: true })
            {
                continue;
            }

            EnemyIntentPlanner.Plan(enemy, _player, _grid);

            var intent = enemy.Get<Intent>()!;
            var declared = new IntentDeclaredEvent(enemy, intent);
            Journal.Record(declared);
            EventBus.Publish(declared);
        }
    }

    private void ExecuteIntent(Entity enemy)
    {
        var intent = enemy.Get<Intent>();
        if (intent is null) { return; }

        switch (intent.Kind)
        {
            // Intent was declared at the start of the round, before the
            // player acted -- the tile it was aiming for may have filled up
            // since (most commonly: the player moved there). Re-check
            // occupancy rather than trusting the stale plan; if it's no
            // longer clear, the enemy just stands still this turn.
            case IntentKind.Move when intent.TargetPosition is { } destination && !IsOccupied(destination):
                MoveTo(enemy, destination);
                break;

            case IntentKind.Attack when intent.TargetEntityId is { } targetId:
                var target = FindEntity(targetId);
                if (target is not null)
                {
                    CastBasicAttack(enemy, target);
                }

                break;
        }

        intent.Clear();
        CheckForCombatEnd();
    }

    /// <remarks>
    /// Every basic attack -- player or enemy -- casts the same composed
    /// spell (CombatSetup.BasicAttack), resolving each of its effects in
    /// order. A composed spell can carry more than one effect (e.g. damage
    /// plus a Flame Rune's applied Burn), so this is a loop even when the
    /// content behind it has exactly one.
    /// </remarks>
    private void CastBasicAttack(Entity caster, Entity target)
    {
        foreach (var effect in _basicAttack.Effects)
        {
            _resolver.Resolve(
                effect,
                new EffectContext(caster, new[] { target }, _random, depth: 0, EffectOrigin.Direct));
        }
    }

    /// <remarks>
    /// The only place that resolves a MoveEffect. Every caller must check
    /// IsOccupied against the destination first -- Grid itself tracks no
    /// occupancy (src/Relicbound.Core/Rules/Grid.cs), so this is the one
    /// seam that keeps two entities from ever sharing a tile.
    /// </remarks>
    private void MoveTo(Entity actor, GridPoint destination)
    {
        _resolver.Resolve(
            new MoveEffect(destination),
            new EffectContext(actor, new[] { actor }, _random, depth: 0, EffectOrigin.Direct));
    }

    private bool IsOccupied(GridPoint point)
    {
        return _entities.Any(e =>
        {
            var health = e.Get<Health>();
            if (health is not null && health.IsDefeated)
            {
                return false;
            }

            var pos = e.Get<GridPosition>();
            return pos is not null && pos.Point == point;
        });
    }

    private Entity? FindEntity(EntityId id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }

    private void CheckForCombatEnd()
    {
        if (_player.Get<Health>() is { IsDefeated: true })
        {
            Winner = CombatWinner.Enemies;
            return;
        }

        if (_enemies.All(e => e.Get<Health>() is { IsDefeated: true }))
        {
            Winner = CombatWinner.Player;
        }
    }
}
