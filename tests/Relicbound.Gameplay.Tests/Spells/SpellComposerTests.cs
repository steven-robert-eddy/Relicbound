using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Runes;
using Relicbound.Gameplay.Spells;
using Xunit;

namespace Relicbound.Gameplay.Tests.Spells;

public class SpellComposerTests
{
    private static SpellDefinition Bolt() => new(
        Id: "bolt",
        Name: "Bolt",
        Tags: new[] { Tag.Void },
        Cost: 1,
        Range: 4,
        Sockets: 2,
        Targeting: TargetingMode.Single,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Damage, Value: 6) });

    private static RuneDefinition FlameRune() => new(
        Id: "rune_flame",
        Name: "Flame Rune",
        NamePrefix: "Inferno",
        AddsTags: new[] { Tag.Fire },
        CostDelta: 0,
        AddsEffects: new[]
        {
            new EffectDefinition(EffectDefinitionType.ApplyStatus, Status: StatusType.Burn, Stacks: 2, DurationRounds: 3),
        });

    private static RuneDefinition ChainRune() => new(
        Id: "rune_chain",
        Name: "Chain Rune",
        NamePrefix: "Chain",
        AddsTags: System.Array.Empty<Tag>(),
        CostDelta: 1,
        AddsEffects: System.Array.Empty<EffectDefinition>(),
        ChainAdditionalTargets: 2,
        ChainFalloffPercent: 50);

    [Fact]
    public void Compose_BoltFlameChain_ProducesInfernoChainBoltName()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune(), ChainRune() });

        Assert.Equal("Inferno Chain Bolt", composed.Name);
    }

    [Fact]
    public void Compose_CostIsBasePlusDeltas()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune(), ChainRune() });

        Assert.Equal(2, composed.Cost); // 1 (base) + 0 (flame) + 1 (chain)
    }

    [Fact]
    public void Compose_CostFloorsAtOne()
    {
        var cheapeningRune = new RuneDefinition(
            Id: "rune_cheap",
            Name: "Cheapening Rune",
            NamePrefix: "Cheap",
            AddsTags: System.Array.Empty<Tag>(),
            CostDelta: -5,
            AddsEffects: System.Array.Empty<EffectDefinition>());

        var composed = SpellComposer.Compose(Bolt(), new[] { cheapeningRune });

        Assert.Equal(1, composed.Cost);
    }

    [Fact]
    public void Compose_TagsAreUnionOfBaseAndRuneTags_Deduplicated()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune(), ChainRune() });

        Assert.Equal(new[] { Tag.Void, Tag.Fire }, composed.Tags);
    }

    [Fact]
    public void Compose_EveryCompiledEffect_CarriesTheFullComposedTagSet()
    {
        // The point of the whole mechanic: Bolt's own DamageEffect was
        // authored with tag Void, but once a Flame Rune is socketed, that
        // same DamageEffect instance must carry Fire too -- not just the
        // Burn effect the rune itself contributed. This is what lets a
        // Fire-requiring artifact react to a Flame-Runed Bolt.
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune() });

        Assert.All(composed.Effects, effect => Assert.Contains(Tag.Fire, effect.Tags));

        var damageEffect = Assert.IsType<DamageEffect>(composed.Effects[0]);
        Assert.Equal(6, damageEffect.Amount);
        Assert.Contains(Tag.Void, damageEffect.Tags);
        Assert.Contains(Tag.Fire, damageEffect.Tags);
    }

    [Fact]
    public void Compose_ChainRune_SetsTargetingToChain_WithTargetsAndFalloff()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { ChainRune() });

        Assert.Equal(TargetingMode.Chain, composed.Targeting);
        Assert.Equal(2, composed.ChainAdditionalTargets);
        Assert.Equal(50, composed.ChainFalloffPercent);
    }

    [Fact]
    public void Compose_WithoutChainRune_KeepsBaseTargeting()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune() });

        Assert.Equal(TargetingMode.Single, composed.Targeting);
        Assert.Null(composed.ChainAdditionalTargets);
        Assert.Null(composed.ChainFalloffPercent);
    }

    [Fact]
    public void Compose_EffectCount_IsBaseEffectsPlusEachRunesAddedEffects()
    {
        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune(), ChainRune() });

        // Bolt's 1 damage effect + Flame's 1 Burn effect + Chain's 0 effects.
        Assert.Equal(2, composed.Effects.Count);
        Assert.IsType<DamageEffect>(composed.Effects[0]);
        Assert.IsType<ApplyStatusEffect>(composed.Effects[1]);
    }

    [Fact]
    public void Compose_MoreRunesThanSockets_Throws()
    {
        var spell = Bolt() with { Sockets = 1 };

        Assert.Throws<System.InvalidOperationException>(
            () => SpellComposer.Compose(spell, new[] { FlameRune(), ChainRune() }));
    }
}
