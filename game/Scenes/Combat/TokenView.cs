using Godot;

namespace Relicbound.Game.Scenes.Combat;

/// <remarks>
/// A placeholder visual for one entity: a colored square plus a letter
/// label, matching the ASCII mock in docs/GAME_DESIGN.md. Both children set
/// MouseFilter to Ignore so a click on a token still reaches
/// CombatSandboxView._UnhandledInput instead of being consumed here.
/// </remarks>
public sealed class TokenView
{
    private readonly ColorRect _square;

    public TokenView(Node2D parent, float tileSize, Color color, string letter)
    {
        _square = new ColorRect
        {
            Color = color,
            Size = new Vector2(tileSize, tileSize),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        parent.AddChild(_square);

        var label = new Label
        {
            Text = letter,
            Size = new Vector2(tileSize, tileSize),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _square.AddChild(label);
    }

    public void SetPosition(Vector2 topLeft)
    {
        _square.Position = topLeft;
    }

    public void SetVisible(bool visible)
    {
        _square.Visible = visible;
    }
}
