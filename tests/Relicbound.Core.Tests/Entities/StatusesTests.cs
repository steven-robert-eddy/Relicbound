using System.Linq;
using Relicbound.Core.Entities;
using Xunit;

namespace Relicbound.Core.Tests.Entities;

public class StatusesTests
{
    [Fact]
    public void Add_NewStatus_IsTrackedWithGivenStacksAndDuration()
    {
        var statuses = new Statuses();

        var stacked = statuses.Add(StatusType.Burn, stacks: 2, durationRounds: 3);

        Assert.False(stacked);
        var burn = statuses.Get(StatusType.Burn);
        Assert.NotNull(burn);
        Assert.Equal(2, burn!.Stacks);
        Assert.Equal(3, burn.RemainingRounds);
    }

    [Fact]
    public void Add_ExistingStatus_SumsStacks_AndRefreshesToLongerDuration()
    {
        var statuses = new Statuses();
        statuses.Add(StatusType.Burn, stacks: 2, durationRounds: 1);

        var stacked = statuses.Add(StatusType.Burn, stacks: 3, durationRounds: 4);

        Assert.True(stacked);
        var burn = statuses.Get(StatusType.Burn);
        Assert.Equal(5, burn!.Stacks);
        Assert.Equal(4, burn.RemainingRounds);
    }

    [Fact]
    public void Add_ExistingStatus_DoesNotShortenLongerRemainingDuration()
    {
        var statuses = new Statuses();
        statuses.Add(StatusType.Burn, stacks: 1, durationRounds: 5);

        statuses.Add(StatusType.Burn, stacks: 1, durationRounds: 1);

        Assert.Equal(5, statuses.Get(StatusType.Burn)!.RemainingRounds);
    }

    [Fact]
    public void TickDurations_DecrementsRemainingRounds()
    {
        var statuses = new Statuses();
        statuses.Add(StatusType.Burn, stacks: 1, durationRounds: 2);

        var expired = statuses.TickDurations();

        Assert.Empty(expired);
        Assert.Equal(1, statuses.Get(StatusType.Burn)!.RemainingRounds);
    }

    [Fact]
    public void TickDurations_RemovesAndReportsStatus_WhenDurationReachesZero()
    {
        var statuses = new Statuses();
        statuses.Add(StatusType.Burn, stacks: 1, durationRounds: 1);

        var expired = statuses.TickDurations();

        Assert.False(statuses.Has(StatusType.Burn));
        var expiredStatus = Assert.Single(expired);
        Assert.Equal(StatusType.Burn, expiredStatus.Type);
    }

    [Fact]
    public void Active_ExposesAllTrackedStatuses()
    {
        var statuses = new Statuses();
        statuses.Add(StatusType.Burn, stacks: 1, durationRounds: 1);

        Assert.Single(statuses.Active);
        Assert.Equal(StatusType.Burn, statuses.Active.Single().Type);
    }
}
