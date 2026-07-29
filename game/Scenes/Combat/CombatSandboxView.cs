using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Relicbound.Content;
using Relicbound.Content.Artifacts;
using Relicbound.Content.Runes;
using Relicbound.Content.Spells;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Combat;
using Relicbound.Gameplay.Runes;
using Relicbound.Gameplay.Spells;

namespace Relicbound.Game.Scenes.Combat;

/// <remarks>
/// Owns one CombatSimulation, turns clicks into RequestMove/RequestAttack
/// calls, and re-renders from the Journal and entity state after every
/// action. No gameplay rules live here — this only displays what the
/// simulation already decided. See docs/CODING_STANDARDS.md.
///
/// Before a fight starts, it also owns the Spell Forge panel (Milestone 3
/// issue 3.9): socket/unsocket buttons mutate only which RuneDefinition
/// occupies which slot in this view, and the preview label is recomputed by
/// calling the real SpellComposer.Compose on every change -- composition
/// itself stays entirely in Gameplay. "Start Fight" composes once more and
/// that composed spell becomes the fight's basic attack.
/// </remarks>
public partial class CombatSandboxView : Node2D
{
    private CombatSimulation? _simulation;
    private GridView? _gridView;
    private Node2D? _tokensLayer;
    private RichTextLabel? _combatLog;
    private Label? _apLabel;
    private Label? _artifactsLabel;
    private Button? _endTurnButton;
    private Button? _socket1Button;
    private Button? _socket2Button;
    private Label? _previewLabel;
    private Button? _startFightButton;

    private readonly Dictionary<EntityId, TokenView> _tokens = new();
    private int _journalEntriesRendered;
    private EntityId _playerId;
    private EntityId _enemyId;

    // The spell the forge panel lets the player socket runes into. Bolt has
    // two sockets, so it's the one that can actually demonstrate the
    // Milestone 3 proof (docs/GAME_DESIGN.md section 7): Bolt + Flame Rune +
    // Chain Rune = Inferno Chain Bolt, carrying the Fire tag.
    private SpellDefinition? _forgeSpell;
    private readonly List<RuneDefinition> _availableRunes = new();
    private RuneDefinition?[] _sockets = Array.Empty<RuneDefinition?>();

    public override void _Ready()
    {
        _gridView = GetNode<GridView>("GridView");
        _tokensLayer = GetNode<Node2D>("GridView/TokensLayer");
        _combatLog = GetNode<RichTextLabel>("UI/CombatLog");
        _apLabel = GetNode<Label>("UI/APLabel");
        _artifactsLabel = GetNode<Label>("UI/ArtifactsLabel");
        _endTurnButton = GetNode<Button>("UI/EndTurnButton");
        _socket1Button = GetNode<Button>("UI/SpellForge/Socket1Button");
        _socket2Button = GetNode<Button>("UI/SpellForge/Socket2Button");
        _previewLabel = GetNode<Label>("UI/SpellForge/PreviewLabel");
        _startFightButton = GetNode<Button>("UI/SpellForge/StartFightButton");

        _endTurnButton.Pressed += OnEndTurnPressed;

        var spells = SpellContentLoader.LoadEmbedded(ContentAssembly.Reference);
        _forgeSpell = spells.First(s => s.Id == "bolt");
        _availableRunes.AddRange(RuneContentLoader.LoadEmbedded(ContentAssembly.Reference));
        _sockets = new RuneDefinition?[_forgeSpell.Sockets];

        _socket1Button.Pressed += () => OnSocketButtonPressed(0);
        _socket2Button.Pressed += () => OnSocketButtonPressed(1);
        _startFightButton.Pressed += OnStartFightPressed;

        RefreshForge();
    }

    /// <remarks>
    /// Clicking a socket cycles it through "empty", then every rune not
    /// already socketed elsewhere, back to "empty" -- one control does both
    /// socketing and unsocketing. A rune can't occupy two sockets at once;
    /// that's a UI-level constraint, not a rule SpellComposer enforces.
    /// </remarks>
    private void OnSocketButtonPressed(int socketIndex)
    {
        var usedElsewhere = _sockets
            .Where((rune, i) => i != socketIndex && rune is not null)
            .Select(rune => rune!)
            .ToList();

        var options = new List<RuneDefinition?> { null };
        options.AddRange(_availableRunes.Where(r => !usedElsewhere.Contains(r)));

        var currentIndex = options.IndexOf(_sockets[socketIndex]);
        _sockets[socketIndex] = options[(currentIndex + 1) % options.Count];

        RefreshForge();
    }

    private void RefreshForge()
    {
        if (_forgeSpell is null) { return; }

        var socketButtons = new[] { _socket1Button!, _socket2Button! };
        for (var i = 0; i < socketButtons.Length; i++)
        {
            var rune = i < _sockets.Length ? _sockets[i] : null;
            socketButtons[i].Text = $"Socket {i + 1}: {(rune is null ? "(empty)" : rune.Name)}";
        }

        var composed = SpellComposer.Compose(_forgeSpell, SocketedRunes());
        var tags = composed.Tags.Count == 0 ? "(none)" : string.Join(", ", composed.Tags);
        _previewLabel!.Text = $"Preview: {composed.Name} — {composed.Cost} AP — Tags: {tags}";
    }

    private void OnStartFightPressed()
    {
        if (_forgeSpell is null) { return; }

        var composed = SpellComposer.Compose(_forgeSpell, SocketedRunes());

        _socket1Button!.Disabled = true;
        _socket2Button!.Disabled = true;
        _startFightButton!.Disabled = true;

        StartNewFight(composed);
    }

    private List<RuneDefinition> SocketedRunes()
    {
        return _sockets.Where(r => r is not null).Select(r => r!).ToList();
    }

    private void StartNewFight(ComposedSpell basicAttack)
    {
        var player = new Entity(new EntityId(1), "Player");
        player.Add(new PlayerControlled());
        player.Add(new Health(30));
        player.Add(new GridPosition(new GridPoint(1, 3)));

        // A live demonstration of Milestone 2's actual proof: equipping
        // Ember Heart changes combat with no code written for it
        // specifically -- it's data, loaded the same way it would be
        // in a real Workshop scene.
        var artifacts = ArtifactContentLoader.LoadEmbedded(ContentAssembly.Reference);
        var emberHeart = artifacts.FirstOrDefault(a => a.Id == "ember_heart");
        if (emberHeart is not null)
        {
            var equipment = new Equipment();
            equipment.Equip(0, emberHeart);
            player.Add(equipment);
        }

        var goblin = new Entity(new EntityId(2), "Goblin");
        goblin.Add(new Health(20));
        goblin.Add(new GridPosition(new GridPoint(7, 3)));

        _playerId = player.Id;
        _enemyId = goblin.Id;

        var setup = new CombatSetup(
            _gridView!.Columns,
            _gridView.Rows,
            new Entity[] { player, goblin },
            (ulong)DateTimeOffset.UtcNow.Ticks,
            basicAttack);

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

        var player = FindEntity(_playerId);
        var equipment = player?.Get<Equipment>();
        var equippedNames = equipment is null
            ? Enumerable.Empty<string>()
            : equipment.Slots.Where(a => a is not null).Select(a => a!.Name);
        _artifactsLabel!.Text = "Artifact: " + (equippedNames.Any() ? string.Join(", ", equippedNames) : "(none)");

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
            StatusAppliedEvent e => $"{e.Target.Name} is afflicted with {e.Status} ({e.Stacks}).",
            StatusStackedEvent e => $"{e.Target.Name}'s {e.Status} stacks to {e.TotalStacks}.",
            StatusExpiredEvent e => $"{e.Target.Name}'s {e.Status} fades.",
            StatModifiedEvent e => $"{e.Target.Name}'s {e.Stat} is modified.",
            ArtifactTriggeredEvent e => $"{e.Holder.Name}'s {e.ArtifactId} triggers!",
            ArtifactEquippedEvent e => $"{e.Holder.Name} equips {e.ArtifactId}.",
            ArtifactUnequippedEvent e => $"{e.Holder.Name} unequips {e.ArtifactId}.",
            EntityRevivedEvent e => $"{e.Target.Name} is revived, gaining {e.Amount} health!",
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
