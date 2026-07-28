using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;

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
    private const int AttackCost = 1;
    private const int StrikeDamage = 5;

    private readonly Grid _grid;
    private readonly IReadOnlyList<Entity> _entities;
    private readonly Entity _player;
    private readonly IReadOnlyList<Entity> _enemies;
    private readonly ITurnResourceModel _turnResourceModel;
    private readonly EffectResolver _resolver;
    private readonly IRandomSource _random;

    public CombatSimulation(CombatSetup setup)
    {
        _grid = new Grid(setup.GridWidth, setup.GridHeight);
        _entities = setup.Entities;
        _player = _entities.Single(e => e.Has<PlayerControlled>());
        _enemies = _entities.Where(e => !e.Has<PlayerControlled>()).ToList();
        _turnResourceModel = new ActionPointModel();
        EventBus = new EventBus();
        Journal = new Journal();
        _resolver = new EffectResolver(EventBus, Journal);
        _random = new SplitMix64RandomSource(setup.RandomSeed);

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
        }

        StartRound();
    }

    public IEventBus EventBus { get; }
    public Journal Journal { get; }
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
        _resolver.Resolve(
            new MoveEffect(destination),
            new EffectContext(actor, new[] { actor }, _random, depth: 0, EffectOrigin.Direct));

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

        var cost = new ActionCost(AttackCost);
        if (!_turnResourceModel.CanAfford(actor, cost)) { return false; }

        _turnResourceModel.Spend(actor, cost);
        _resolver.Resolve(
            new DamageEffect(StrikeDamage),
            new EffectContext(actor, new[] { target }, _random, depth: 0, EffectOrigin.Direct));

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
        foreach (var entity in _entities)
        {
            _turnResourceModel.Replenish(entity);

            var statuses = entity.Get<Statuses>();
            if (statuses is null) { continue; }

            foreach (var expired in statuses.TickDurations())
            {
                var expiredEvent = new StatusExpiredEvent(entity, expired.Type);
                Journal.Record(expiredEvent);
                EventBus.Publish(expiredEvent);
            }
        }

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
            case IntentKind.Move when intent.TargetPosition is { } destination:
                _resolver.Resolve(
                    new MoveEffect(destination),
                    new EffectContext(enemy, new[] { enemy }, _random, depth: 0, EffectOrigin.Direct));
                break;

            case IntentKind.Attack when intent.TargetEntityId is { } targetId:
                var target = FindEntity(targetId);
                if (target is not null)
                {
                    _resolver.Resolve(
                        new DamageEffect(StrikeDamage),
                        new EffectContext(enemy, new[] { target }, _random, depth: 0, EffectOrigin.Direct));
                }

                break;
        }

        intent.Clear();
        CheckForCombatEnd();
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
