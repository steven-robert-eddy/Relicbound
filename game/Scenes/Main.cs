using Godot;

namespace Relicbound.Game.Scenes;

public partial class Main : Node2D
{
    // Milestone 4 issue 4.3: the playable loop starts on the expedition
    // map now, not straight into a single hardcoded fight. It'll route to
    // the Workshop first instead, once that exists (Milestone 4 issue 4.6).
    private const string ExpeditionMapScenePath = "res://Scenes/Expeditions/ExpeditionMapView.tscn";

    public override void _Ready()
    {
        var expeditionMap = GD.Load<PackedScene>(ExpeditionMapScenePath).Instantiate();
        AddChild(expeditionMap);
    }
}
