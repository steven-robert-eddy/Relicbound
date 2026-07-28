using Relicbound.Core.Stats;
using Xunit;

namespace Relicbound.Core.Tests.Stats;

public class StatBlockTests
{
    [Fact]
    public void GetValue_WithNoModifiers_ReturnsBaseValue()
    {
        var stats = new StatBlock();
        stats.SetBase(StatType.AttackPower, 10);

        Assert.Equal(10, stats.GetValue(StatType.AttackPower));
    }

    [Fact]
    public void GetValue_AppliesFlatThenIncreasedThenMore_InThatOrder()
    {
        var stats = new StatBlock();
        stats.SetBase(StatType.AttackPower, 100);
        stats.AddModifier(new StatModifier(StatType.AttackPower, ModifierLayer.Flat, 20, new ModifierSource("a")));
        stats.AddModifier(new StatModifier(StatType.AttackPower, ModifierLayer.Increased, 50, new ModifierSource("b")));
        stats.AddModifier(new StatModifier(StatType.AttackPower, ModifierLayer.More, 20, new ModifierSource("c")));

        // (100 + 20) * 1.5 * 1.2 = 216
        Assert.Equal(216, stats.GetValue(StatType.AttackPower));
    }

    [Fact]
    public void RemoveModifiersFrom_RemovesOnlyThatSourcesModifiers()
    {
        var stats = new StatBlock();
        stats.SetBase(StatType.AttackPower, 10);
        stats.AddModifier(new StatModifier(StatType.AttackPower, ModifierLayer.Flat, 5, new ModifierSource("temp")));
        stats.AddModifier(new StatModifier(StatType.AttackPower, ModifierLayer.Flat, 3, new ModifierSource("permanent")));

        stats.RemoveModifiersFrom(new ModifierSource("temp"));

        Assert.Equal(13, stats.GetValue(StatType.AttackPower));
    }
}
