using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Relicbound.Content;
using Relicbound.Content.Encounters;
using Relicbound.Core.Rules;
using Relicbound.Game.Scenes.Combat;
using Relicbound.Gameplay.Encounters;
using Relicbound.Gameplay.Expeditions;

namespace Relicbound.Game.Scenes.Expeditions;

/// <remarks>
/// Milestone 4 issue 4.3, made playable. Owns one Expedition (Gameplay) and
/// renders its map as a grid of buttons -- Start at the bottom, Boss at the
/// top, matching docs/GAME_DESIGN.md section 8's sketch. Clicking a
/// reachable node calls Expedition.EnterNode; if that node needs a fight,
/// this instantiates CombatSandbox.tscn, configures it with the node's
/// encounter, and swaps to it until combat resolves.
///
/// One root random stream (docs/TECHNICAL_ARCHITECTURE.md section 2) hands
/// out the map's own seed and, moving forward, one seed per fight via
/// NextSeed() -- map generation and every combat draw from independent,
/// derived streams rather than sharing one.
///
/// Player health persists across nodes within a run (reset only on a fresh
/// expedition) so "complete or die" means something -- without it, every
/// fight would start full-health and dying would be nearly impossible.
/// </remarks>
public partial class ExpeditionMapView : Node2D
{
    private const string CombatSandboxScenePath = "res://Scenes/Combat/CombatSandbox.tscn";
    private const int PlayerMaxHealth = 30;

    private const float TopMargin = 60f;
    private const float RowSpacing = 110f;
    private const float LeftMargin = 60f;
    private const float ColumnSpacing = 160f;
    private const float ButtonWidth = 140f;
    private const float ButtonHeight = 60f;

    private Label? _statusLabel;
    private Control? _nodeButtonsRoot;
    private Button? _restartButton;
    private Node2D? _combatHost;

    private IReadOnlyList<EncounterDefinition> _encounters = Array.Empty<EncounterDefinition>();
    private readonly Dictionary<ExpeditionNodeId, Button> _nodeButtons = new();

    private SplitMix64RandomSource _rootRandom = new(1);
    private Expedition? _expedition;
    private CombatSandboxView? _activeCombat;
    private int _playerHealth = PlayerMaxHealth;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("MapUI/StatusLabel");
        _nodeButtonsRoot = GetNode<Control>("MapUI/NodeButtonsRoot");
        _restartButton = GetNode<Button>("MapUI/RestartButton");
        _combatHost = GetNode<Node2D>("CombatHost");

        _restartButton.Pressed += OnRestartPressed;
        _encounters = EncounterContentLoader.LoadEmbedded(ContentAssembly.Reference);

        StartNewExpedition();
    }

    private void StartNewExpedition()
    {
        _rootRandom = new SplitMix64RandomSource((ulong)DateTimeOffset.UtcNow.Ticks);
        _playerHealth = PlayerMaxHealth;
        _restartButton!.Visible = false;

        var map = ExpeditionMapGenerator.Generate(_rootRandom.NextSeed(), encounters: _encounters);
        _expedition = new Expedition(map);

        BuildNodeButtons(map);
        SetStatus("Expedition begins. Choose your first step.");
        RefreshNodeButtons();
    }

    private void BuildNodeButtons(ExpeditionMap map)
    {
        foreach (var button in _nodeButtons.Values)
        {
            button.QueueFree();
        }

        _nodeButtons.Clear();

        var maxFloor = map.Nodes.Max(n => n.Floor);
        var floors = map.Nodes.GroupBy(n => n.Floor).ToDictionary(g => g.Key, g => g.OrderBy(n => n.Id.Value).ToList());

        foreach (var (floor, nodes) in floors)
        {
            var y = TopMargin + (maxFloor - floor) * RowSpacing;

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var x = LeftMargin + i * ColumnSpacing;

                var button = new Button
                {
                    Text = NodeLabel(node),
                    Position = new Vector2(x, y),
                    Size = new Vector2(ButtonWidth, ButtonHeight),
                };

                var nodeId = node.Id;
                button.Pressed += () => OnNodeButtonPressed(nodeId);

                _nodeButtonsRoot!.AddChild(button);
                _nodeButtons[node.Id] = button;
            }
        }
    }

    private string NodeLabel(ExpeditionNode node)
    {
        if (node.EncounterId is null)
        {
            return node.Type.ToString();
        }

        var encounterName = _encounters.FirstOrDefault(e => e.Id == node.EncounterId)?.Name ?? node.EncounterId;
        return $"{node.Type}\n{encounterName}";
    }

    private void RefreshNodeButtons()
    {
        if (_expedition is null) { return; }

        var reachable = _expedition.CurrentNodeIsCleared
            ? new HashSet<ExpeditionNodeId>(_expedition.CurrentNode.NextNodeIds)
            : new HashSet<ExpeditionNodeId>();

        foreach (var (nodeId, button) in _nodeButtons)
        {
            if (nodeId == _expedition.CurrentNodeId)
            {
                button.Modulate = new Color(1f, 0.9f, 0.3f);
                button.Disabled = true;
            }
            else if (reachable.Contains(nodeId) && _expedition.Status == ExpeditionStatus.InProgress)
            {
                button.Modulate = Colors.White;
                button.Disabled = false;
            }
            else
            {
                button.Modulate = new Color(0.5f, 0.5f, 0.5f);
                button.Disabled = true;
            }
        }
    }

    private void OnNodeButtonPressed(ExpeditionNodeId nodeId)
    {
        if (_expedition is null || !_expedition.EnterNode(nodeId)) { return; }

        if (_expedition.CurrentNodeRequiresCombat)
        {
            BeginCombatForCurrentNode();
        }
        else
        {
            SetStatus($"Entered a {_expedition.CurrentNode.Type} node. Nothing to resolve here yet -- later milestone work.");
            RefreshNodeButtons();
        }
    }

    private void BeginCombatForCurrentNode()
    {
        var node = _expedition!.CurrentNode;
        var encounter = _encounters.FirstOrDefault(e => e.Id == node.EncounterId);
        var enemies = encounter?.Enemies ?? new[] { new EncounterEnemy("goblin", "Goblin", 20) };

        SetStatus($"{node.Type} encounter: {encounter?.Name ?? "Unknown"}.");
        GetNode<CanvasLayer>("MapUI").Visible = false;

        var combat = GD.Load<PackedScene>(CombatSandboxScenePath).Instantiate<CombatSandboxView>();
        combat.Configure(enemies, _playerHealth, _rootRandom.NextSeed(), OnCombatResolved);
        _combatHost!.AddChild(combat);
        _activeCombat = combat;
    }

    private void OnCombatResolved(bool playerWon)
    {
        if (playerWon && _activeCombat?.PlayerHealthRemaining is { } remaining)
        {
            _playerHealth = remaining;
        }

        _activeCombat?.QueueFree();
        _activeCombat = null;
        GetNode<CanvasLayer>("MapUI").Visible = true;

        _expedition!.ResolveCombat(playerWon);

        if (_expedition.Status != ExpeditionStatus.InProgress)
        {
            SetStatus(_expedition.Status == ExpeditionStatus.Complete
                ? "Expedition complete! The boss falls."
                : "You have died. The expedition ends.");
            _restartButton!.Visible = true;
        }
        else
        {
            SetStatus(playerWon ? "Victory! Choose your next step." : "Defeat.");
        }

        RefreshNodeButtons();
    }

    private void OnRestartPressed()
    {
        StartNewExpedition();
    }

    private void SetStatus(string text)
    {
        _statusLabel!.Text = text;
    }
}
