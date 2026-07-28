using Relicbound.Core.Entities;
using Xunit;

namespace Relicbound.Core.Tests.Entities;

public class HealthTests
{
    [Fact]
    public void ApplyDamage_ReducesCurrent()
    {
        var health = new Health(30);

        health.ApplyDamage(10);

        Assert.Equal(20, health.Current);
    }

    [Fact]
    public void ApplyDamage_ExceedingCurrent_ClampsAtZero_AndIsDefeated()
    {
        var health = new Health(10);

        health.ApplyDamage(50);

        Assert.Equal(0, health.Current);
        Assert.True(health.IsDefeated);
    }

    [Fact]
    public void ApplyDamage_WithShield_AbsorbsBeforeCurrent()
    {
        var health = new Health(30);
        health.AddShield(10);

        health.ApplyDamage(15);

        Assert.Equal(0, health.Shield);
        Assert.Equal(25, health.Current);
    }

    [Fact]
    public void Heal_ClampsAtMax()
    {
        var health = new Health(30);
        health.ApplyDamage(5);

        health.Heal(100);

        Assert.Equal(30, health.Current);
    }
}
