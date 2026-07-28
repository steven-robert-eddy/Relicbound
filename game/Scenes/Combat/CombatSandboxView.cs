using System;
using System.Collections.Generic;
using Godot;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Gameplay.Combat;

namespace Relicbound.Game.Scenes.Combat;

/// <remarks>
/// Owns one CombatSimulation, turns clicks into RequestMove/RequestAttack
/// calls, and re-renders from the Journal and entity state after every
/// action. No gameplay rules live here — this only displays what the
/// simulation already decided. See docs/CODING_STANDARDS.md.
/// </remarks>
public partial class CombatSandboxView : Node2D
{
    private CombatSimulation? _simulation;
    private GridView? _gridView;
    private Node2D? _tokensLayer;
    private RichTextLabel? _combatLog;
    private Label? _apLabel;
    private Button? _endTurnButton;

    private readonly Dictionary<EntityId, TokenView> _tokens = new();
    private int _journalEntriesRendered;
    private EntityId _playerId;
    private EntityId _enemyId;

    public override void _Ready()
    {
        _gridView = GetNode<GridView>("GridView");
        _tokensLayer = GetNode<Node2D>("GridView/TokensLayer");
        _combatLog = GetNode<RichTextLabel>("UI/CombatLog");
        _apLabel = GetNode<Label>("UI/APLabel");
        _endTurnButton = GetNode<Button>("UI/EndTurnButton");

        _endTurnButton.Pressed += OnEndTurnPressed;

        StartNewFight();
    }

    private void StartNewFight()
    {
        var player = new Entity(new EntityId(1), "Player");
        player.Add(new PlayerControlled());
        player.Add(new Health(30));
        player.Add(new GridPosition(new GridPoint(1, 3)));

        var goblin = new Entity(new EntityId(2), "Goblin");
        goblin.Add(new Health(20));
        goblin.Add(new GridPosition(new GridPoint(7, 3)));

        _playerId = player.Id;
        _enemyId = goblin.Id;

        var setup = new CombatSetup(
            _gridView!.Columns,
            _gridView.Rows,
            new Entity[] { player, goblin },
            (ulong)DateTimeOffset.UtcNow.Ticks);

        _simulation = new CombatSimulation(setup);
        _journalEntriesRendered = 0;

        foreach (var token in _tokens.Values)
        {
            token.SetVisible(false);
        }

        _tokens.Clear();

        foreach (var entity in _simulation.Entities)
        {
            var isPlayer = entity.Has<PlayerControlled>();
            var color = isPlayer ? new Color(0.3f, 0.5f, 0.9f) : new Color(0.85f, 0.25f, 0.25f);
            var letter = isPlayer ? "P" : "E";
            _tokens[entity.Id] = new TokenView(_tokensLayer!, _gridView.TileSize, color, letter);
        }

        RenderState();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_simulation is null || _simulation.IsCombatOver) { return; }

        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton)
        {
            return;
        }

        var localPosition = _gridView!.ToLocal(mouseButton.GlobalPosition);
        var tile = _gridView.PixelToTile(localPosition);

        var enemy = FindEntity(_enemyId);
        var enemyTile = enemy?.Get<GridPosition>()?.Point;

        var handled = enemyTile is { } occupiedTile && occupiedTile == tile
            ? _simulation.RequestAttack(_playerId, _enemyId)
            : _simulation.RequestMove(_playerId, tile);

        if (handled)
        {
            AutoEndTurnIfExhausted();
            RenderState();
        }
    }

    private void OnEndTurnPressed()
    {
        if (_simulation is null || _simulation.IsCombatOver) { return; }

        _simulation.EndPlayerTurn();
        RenderState();
    }

    private void AutoEndTurnIfExhausted()
    {
        if (_simulation is { IsCombatOver: false, PlayerActionPoints: <= 0 })
        {
            _simulation.EndPlayerTurn();
        }
    }

    private Entity? FindEntity(EntityId id)
    {
        if (_simulation is null) { return null; }

        foreach (var entity in _simulation.Entities)
        {
            if (entity.Id == id) { return entity; }
        }

        return null;
    }

    private void RenderState()
    {
        if (_simulation is null) { return; }

        foreach (var entity in _simulation.Entities)
        {
            if (!_tokens.TryGetValue(entity.Id, out var token)) { continue; }

            var health = entity.Get<Health>();
            if (health is { IsDefeated: true })
            {
                token.SetVisible(false);
                continue;
            }

            var position = entity.Get<GridPosition>();
            if (position is not null)
            {
                token.SetPosition(_gridView!.TileToPixel(position.Point));
            }
        }

        _apLabel!.Text = $"AP: {_simulation.PlayerActionPoints} / {_simulation.PlayerMaxActionPoints}";

        var entries = _simulation.Journal.Entries;
        for (; _journalEntriesRendered < entries.Count; _journalEntriesRendered++)
        {
            _combatLog!.Text += DescribeEvent(entries[_journalEntriesRendered]) + "\n";
        }

        if (_simulation.IsCombatOver)
        {
            var outcome = _simulation.Winner == CombatWinner.Player ? "Victory!" : "Defeat.";
            _combatLog!.Text += $"-- Combat over: {outcome} --\n";
        }
    }

    private static string DescribeEvent(IGameEvent gameEvent)
    {
        return gameEvent switch
        {
            DamageDealtEvent e => $"{e.Source.Name} hits {e.Target.Name} for {e.Amount}.",
            HealedEvent e => $"{e.Target.Name} heals {e.Amount}.",
            EntityDefeatedEvent e => $"{e.Target.Name} is defeated.",
            PlayerDefeatedEvent => "You have fallen.",
            MovedEvent e => $"{e.Target.Name} moves to ({e.To.X}, {e.To.Y}).",
            IntentDeclaredEvent e => DescribeIntent(e),
            _ => gameEvent.Type.ToString(),
        };
    }

    private static string DescribeIntent(IntentDeclaredEvent declared)
    {
        return declared.Intent.Kind switch
        {
            IntentKind.Attack => $"{declared.Source.Name} intends to attack.",
            IntentKind.Move => $"{declared.Source.Name} intends to move.",
            _ => $"{declared.Source.Name} waits.",
        };
    }
}
