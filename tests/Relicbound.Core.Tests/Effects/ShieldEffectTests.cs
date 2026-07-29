using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class ShieldEffectTests
{
    [Fact]
    public void Execute_GrantsShield_AndPublishesShieldGainedEvent()
    {
        var entity = new Entity(new EntityId(1), "Player");
        entity.Add(new Health(30));
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(entity, new[] { entity }, random, depth: 0, EffectOrigin.Direct);

        var result = new ShieldEffect(8).Execute(context);

        Assert.Equal(8, entity.Get<Health>()!.Shield);
        var shieldEvent = Assert.IsType<ShieldGainedEvent>(Assert.Single(result.Events));
        Assert.Equal(8, shieldEvent.Amount);
    }

    [Fact]
    public void Execute_ShieldAbsorbsSubsequentDamage()
    {
        var entity = new Entity(new EntityId(1), "Player");
        entity.Add(new Health(30));
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(entity, new[] { entity }, random, depth: 0, EffectOrigin.Direct);

        new ShieldEffect(8).Execute(context);
        new DamageEffect(5).Execute(context);

        Assert.Equal(30, entity.Get<Health>()!.Current);
        Assert.Equal(3, entity.Get<Health>()!.Shield);
    }

    [Fact]
    public void Constructor_WithNonPositiveAmount_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new ShieldEffect(0));
    }
}
