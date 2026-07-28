using System.Collections.Generic;

namespace Relicbound.Core.Rules;

/// <remarks>
/// Bounds and orthogonal-neighbor geometry only — no occupancy tracking.
/// With only a handful of entities in a fight, callers scan entities'
/// GridPosition components directly for "what's on this tile" rather than
/// keeping a second occupancy cache in sync with the real component data.
/// </remarks>
public sealed class Grid
{
    public Grid(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public bool IsInBounds(GridPoint point)
    {
        return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
    }

    public IReadOnlyList<GridPoint> Neighbors(GridPoint point)
    {
        var candidates = new[]
        {
            point with { X = point.X + 1 },
            point with { X = point.X - 1 },
            point with { Y = point.Y + 1 },
            point with { Y = point.Y - 1 },
        };

        var result = new List<GridPoint>(4);
        foreach (var candidate in candidates)
        {
            if (IsInBounds(candidate))
            {
                result.Add(candidate);
            }
        }

        return result;
    }
}
