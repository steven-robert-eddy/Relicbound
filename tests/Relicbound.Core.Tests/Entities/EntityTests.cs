using Relicbound.Core.Entities;
using Xunit;

namespace Relicbound.Core.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void Entity_WithAddedComponent_ReturnsItViaGet()
    {
        var entity = new Entity(new EntityId(1), "Goblin");
        var health = new Health(10);

        entity.Add(health);

        Assert.Same(health, entity.Get<Health>());
        Assert.True(entity.Has<Health>());
    }

    [Fact]
    public void Entity_WithoutComponent_HasReturnsFalse_AndGetReturnsNull()
    {
        var entity = new Entity(new EntityId(1), "Goblin");

        Assert.False(entity.Has<Health>());
        Assert.Null(entity.Get<Health>());
    }
}
