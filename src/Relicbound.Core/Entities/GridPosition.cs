using Relicbound.Core.Rules;

namespace Relicbound.Core.Entities;

public sealed class GridPosition : IComponent
{
    public GridPosition(GridPoint point)
    {
        Point = point;
    }

    public GridPoint Point { get; private set; }

    public void MoveTo(GridPoint point)
    {
        Point = point;
    }
}
