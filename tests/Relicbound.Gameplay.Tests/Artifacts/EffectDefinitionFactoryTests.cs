using Relicbound.Core.Effects;
using Relicbound.Core.Stats;
using Relicbound.Gameplay.Artifacts;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

public class EffectDefinitionFactoryTests
{
    private static readonly ModifierSource Source = new("test_artifact");

    [Fact]
    public void Create_Damage_ReturnsDamageEffectWithGivenAmount()
    {
        var effect = EffectDefinitionFactory.Create(new EffectDefinition(EffectDefinitionType.Damage, Value: 6), Source);

        var damage = Assert.IsType<DamageEffect>(effect);
        Assert.Equal(6, damage.Amount);
    }

    [Fact]
    public void Create_Heal_ReturnsHealEffectWithGivenAmount()
    {
        var effect = EffectDefinitionFactory.Create(new EffectDefinition(EffectDefinitionType.Heal, Value: 4), Source);

        var heal = Assert.IsType<HealEffect>(effect);
        Assert.Equal(4, heal.Amount);
    }

    [Fact]
    public void Create_ApplyStatus_ReturnsApplyStatusEffectWithGivenParameters()
    {
        var definition = new EffectDefinition(
            EffectDefinitionType.ApplyStatus,
            Status: Relicbound.Core.Entities.StatusType.Burn,
            Stacks: 2,
            DurationRounds: 3);

        var effect = EffectDefinitionFactory.Create(definition, Source);

        var applyStatus = Assert.IsType<ApplyStatusEffect>(effect);
        Assert.Equal(Relicbound.Core.Entities.StatusType.Burn, applyStatus.Status);
        Assert.Equal(2, applyStatus.Stacks);
        Assert.Equal(3, applyStatus.DurationRounds);
    }

    [Fact]
    public void Create_ModifyStat_ReturnsModifyStatEffectWithGivenParameters()
    {
        var definition = new EffectDefinition(
            EffectDefinitionType.ModifyStat,
            Value: 5,
            Stat: StatType.AttackPower,
            Layer: ModifierLayer.Flat);

        var effect = EffectDefinitionFactory.Create(definition, Source);

        var modifyStat = Assert.IsType<ModifyStatEffect>(effect);
        Assert.Equal(StatType.AttackPower, modifyStat.Stat);
        Assert.Equal(ModifierLayer.Flat, modifyStat.Layer);
        Assert.Equal(5, modifyStat.Value);
    }

    [Fact]
    public void Create_ApplyStatus_MissingStacks_Throws()
    {
        var definition = new EffectDefinition(
            EffectDefinitionType.ApplyStatus,
            Status: Relicbound.Core.Entities.StatusType.Burn,
            DurationRounds: 3);

        Assert.Throws<System.InvalidOperationException>(() => EffectDefinitionFactory.Create(definition, Source));
    }

    [Fact]
    public void Create_Revive_ReturnsReviveEffectWithGivenPercent()
    {
        var effect = EffectDefinitionFactory.Create(new EffectDefinition(EffectDefinitionType.Revive, Value: 50), Source);

        var revive = Assert.IsType<ReviveEffect>(effect);
        Assert.Equal(50, revive.HealthPercent);
    }
}
