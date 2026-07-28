using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class ApplyStatusEffectTests
{
    private static EffectContext CreateContext(Entity source, Entity target)
    {
        var random = new SplitMix64RandomSource(1);
        return new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);
    }

    [Fact]
    public void Execute_OnEntityWithoutStatuses_CreatesComponent_AndAppliesStatus()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        var context = CreateContext(source, target);

        new ApplyStatusEffect(StatusType.Burn, stacks: 2, durationRounds: 3).Execute(context);

        var burn = target.Get<Statuses>()!.Get(StatusType.Burn);
        Assert.NotNull(burn);
        Assert.Equal(2, burn!.Stacks);
    }

    [Fact]
    public void Execute_FirstApplication_PublishesStatusAppliedEvent()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        var context = CreateContext(source, target);

        var result = new ApplyStatusEffect(StatusType.Burn, stacks: 2, durationRounds: 3).Execute(context);

        var applied = Assert.IsType<StatusAppliedEvent>(Assert.Single(result.Events));
        Assert.Equal(StatusType.Burn, applied.Status);
        Assert.Equal(2, applied.Stacks);
    }

    [Fact]
    public void Execute_OnAlreadyAppliedStatus_PublishesStatusStackedEvent()
    {
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        target.Add(new Statuses());
        target.Get<Statuses>()!.Add(StatusType.Burn, stacks: 1, durationRounds: 1);
        var context = CreateContext(source, target);

        var result = new ApplyStatusEffect(StatusType.Burn, stacks: 2, durationRounds: 1).Execute(context);

        var stacked = Assert.IsType<StatusStackedEvent>(Assert.Single(result.Events));
        Assert.Equal(3, stacked.TotalStacks);
    }
}
