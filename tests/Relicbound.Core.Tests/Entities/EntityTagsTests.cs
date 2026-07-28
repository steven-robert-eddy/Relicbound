using Relicbound.Core.Entities;
using Relicbound.Core.Tags;
using Xunit;

namespace Relicbound.Core.Tests.Entities;

public class EntityTagsTests
{
    [Fact]
    public void Values_ReturnsWhatTheEntityWasGiven()
    {
        var entity = new Entity(new EntityId(1), "Goblin");
        entity.Add(new EntityTags(new[] { Tag.Fire, Tag.Blood }));

        var tags = entity.Get<EntityTags>()!.Values;

        Assert.Contains(Tag.Fire, tags);
        Assert.Contains(Tag.Blood, tags);
    }
}
