using Godot;
using Relicbound.Core.Rules;

namespace Relicbound.Game.Scenes.Combat;

/// <remarks>
/// Draws the grid and converts between tile coordinates and pixels. No
/// TileMapLayer/TileSet yet — the visual target is still the ASCII-mockup
/// stage ("do not start with art"), so plain _Draw() calls are enough.
/// TileMapLayer becomes worth it once real tile art exists.
/// </remarks>
public partial class GridView : Node2D
{
    [Export] public int Columns { get; set; } = 9;
    [Export] public int Rows { get; set; } = 7;
    [Export] public float TileSize { get; set; } = 64f;

    public override void _Ready()
    {
        QueueRedraw();
    }

    public Vector2 TileToPixel(GridPoint point)
    {
        return new Vector2(point.X * TileSize, point.Y * TileSize);
    }

    public GridPoint PixelToTile(Vector2 localPosition)
    {
        // A plain (int) cast truncates toward zero, not floor - identical to
        // floor for positive values but wrong for the margin just outside
        // the grid's top-left corner, where a click produces a small
        // negative coordinate that should map to an out-of-bounds tile.
        var x = (int)System.MathF.Floor(localPosition.X / TileSize);
        var y = (int)System.MathF.Floor(localPosition.Y / TileSize);
        return new GridPoint(x, y);
    }

    public override void _Draw()
    {
        var gridColor = new Color(0.35f, 0.35f, 0.4f);

        for (var x = 0; x <= Columns; x++)
        {
            var from = new Vector2(x * TileSize, 0);
            var to = new Vector2(x * TileSize, Rows * TileSize);
            DrawLine(from, to, gridColor);
        }

        for (var y = 0; y <= Rows; y++)
        {
            var from = new Vector2(0, y * TileSize);
            var to = new Vector2(Columns * TileSize, y * TileSize);
            DrawLine(from, to, gridColor);
        }
    }
}
