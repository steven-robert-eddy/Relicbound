using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Stats;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class ModifyStatEffectTests
{
    private static EffectContext CreateContext(Entity source, Entity target)
    {
        var random = new SplitMix64RandomSource(1);
        return new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);
    }

    [Fact]
    public void Execute_OnEntityWithoutStatBlock_CreatesComponent_AndAppliesModifier()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        var context = CreateContext(source, target);

        new ModifyStatEffect(StatType.AttackPower, ModifierLayer.Flat, 5, new ModifierSource("test")).Execute(context);

        Assert.Equal(5, target.Get<StatBlock>()!.GetValue(StatType.AttackPower));
    }

    [Fact]
    public void Execute_OnExistingStatBlock_StacksWithBaseValue()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        var stats = new StatBlock();
        stats.SetBase(StatType.AttackPower, 10);
        target.Add(stats);
        var context = CreateContext(source, target);

        new ModifyStatEffect(StatType.AttackPower, ModifierLayer.Increased, 30, new ModifierSource("test")).Execute(context);

        Assert.Equal(13, target.Get<StatBlock>()!.GetValue(StatType.AttackPower));
    }

    [Fact]
    public void Execute_PublishesStatModifiedEvent()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        var context = CreateContext(source, target);

        var result = new ModifyStatEffect(StatType.Defense, ModifierLayer.Flat, 2, new ModifierSource("test"))
            .Execute(context);

        var modified = Assert.IsType<StatModifiedEvent>(Assert.Single(result.Events));
        Assert.Equal(StatType.Defense, modified.Stat);
        Assert.Equal(ModifierLayer.Flat, modified.Layer);
        Assert.Equal(2, modified.Value);
    }
}
