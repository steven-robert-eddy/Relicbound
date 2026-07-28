using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class ReviveEffectTests
{
    private static EffectContext CreateContext(Entity target)
    {
        var random = new SplitMix64RandomSource(1);
        return new EffectContext(target, new[] { target }, random, depth: 0, EffectOrigin.Artifact);
    }

    [Fact]
    public void Execute_OnDefeatedEntity_RevivesToPercentOfMaxHealth()
    {
        var entity = new Entity(new EntityId(1), "Player");
        var health = new Health(30);
        health.ApplyDamage(30);
        entity.Add(health);
        Assert.True(health.IsDefeated);

        new ReviveEffect(50).Execute(CreateContext(entity));

        Assert.Equal(15, health.Current);
        Assert.False(health.IsDefeated);
    }

    [Fact]
    public void Execute_OnDefeatedEntity_PublishesEntityRevivedEvent()
    {
        var entity = new Entity(new EntityId(1), "Player");
        var health = new Health(30);
        health.ApplyDamage(30);
        entity.Add(health);

        var result = new ReviveEffect(50).Execute(CreateContext(entity));

        var revived = Assert.IsType<EntityRevivedEvent>(Assert.Single(result.Events));
        Assert.Equal(15, revived.Amount);
    }

    [Fact]
    public void Execute_OnEntityThatIsNotDefeated_DoesNothing()
    {
        var entity = new Entity(new EntityId(1), "Player");
        entity.Add(new Health(30));

        var result = new ReviveEffect(50).Execute(CreateContext(entity));

        Assert.Empty(result.Events);
    }
}
