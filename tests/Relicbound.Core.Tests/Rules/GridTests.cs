using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Rules;

public class GridTests
{
    [Fact]
    public void IsInBounds_WithinDimensions_ReturnsTrue()
    {
        var grid = new Grid(5, 5);

        Assert.True(grid.IsInBounds(new GridPoint(0, 0)));
        Assert.True(grid.IsInBounds(new GridPoint(4, 4)));
    }

    [Fact]
    public void IsInBounds_OutsideDimensions_ReturnsFalse()
    {
        var grid = new Grid(5, 5);

        Assert.False(grid.IsInBounds(new GridPoint(-1, 0)));
        Assert.False(grid.IsInBounds(new GridPoint(5, 0)));
        Assert.False(grid.IsInBounds(new GridPoint(0, 5)));
    }

    [Fact]
    public void Neighbors_CornerTile_ReturnsOnlyTwoInBoundsTiles()
    {
        var grid = new Grid(3, 3);

        var neighbors = grid.Neighbors(new GridPoint(0, 0));

        Assert.Equal(2, neighbors.Count);
        Assert.Contains(new GridPoint(1, 0), neighbors);
        Assert.Contains(new GridPoint(0, 1), neighbors);
    }

    [Fact]
    public void Neighbors_InteriorTile_ReturnsFourTiles()
    {
        var grid = new Grid(5, 5);

        var neighbors = grid.Neighbors(new GridPoint(2, 2));

        Assert.Equal(4, neighbors.Count);
    }
}
