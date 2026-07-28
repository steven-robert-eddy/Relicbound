using System;

namespace Relicbound.Core.Rules;

public readonly record struct GridPoint(int X, int Y)
{
    public int ManhattanDistance(GridPoint other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }
}
