using Relicbound.Core.Tags;
using Xunit;

namespace Relicbound.Core.Tests.Tags;

public class TagMatchingTests
{
    [Fact]
    public void Overlaps_WithSharedTag_ReturnsTrue()
    {
        var a = new[] { Tag.Fire, Tag.Spirit };
        var b = new[] { Tag.Spirit, Tag.Ice };

        Assert.True(TagMatching.Overlaps(a, b));
    }

    [Fact]
    public void Overlaps_WithNoSharedTag_ReturnsFalse()
    {
        var a = new[] { Tag.Fire };
        var b = new[] { Tag.Ice };

        Assert.False(TagMatching.Overlaps(a, b));
    }

    [Fact]
    public void Overlaps_WithEmptySet_ReturnsFalse()
    {
        var a = System.Array.Empty<Tag>();
        var b = new[] { Tag.Fire };

        Assert.False(TagMatching.Overlaps(a, b));
    }
}
