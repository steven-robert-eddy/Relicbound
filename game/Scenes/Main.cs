using Godot;

namespace Relicbound.Game.Scenes;

public partial class Main : Node2D
{
    private const string CombatSandboxScenePath = "res://Scenes/Combat/CombatSandbox.tscn";

    public override void _Ready()
    {
        var combatSandbox = GD.Load<PackedScene>(CombatSandboxScenePath).Instantiate();
        AddChild(combatSandbox);
    }
}
