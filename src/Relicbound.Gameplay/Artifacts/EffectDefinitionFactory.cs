using System;
using Relicbound.Core.Effects;
using Relicbound.Core.Stats;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Compiles data into the real, generic Core effects -- the bridge between
/// "content is JSON" and "gameplay changes are Effects". Throwing on a
/// missing field here is a last-resort safety net; ArtifactContentLoader is
/// expected to have already validated every field this needs.
/// </remarks>
public static class EffectDefinitionFactory
{
    public static Effect Create(EffectDefinition definition, ModifierSource source)
    {
        return definition.Type switch
        {
            EffectDefinitionType.Damage => new DamageEffect(Require(definition.Value, nameof(definition.Value))),
            EffectDefinitionType.Heal => new HealEffect(Require(definition.Value, nameof(definition.Value))),
            EffectDefinitionType.ApplyStatus => new ApplyStatusEffect(
                Require(definition.Status, nameof(definition.Status)),
                Require(definition.Stacks, nameof(definition.Stacks)),
                Require(definition.DurationRounds, nameof(definition.DurationRounds))),
            EffectDefinitionType.ModifyStat => new ModifyStatEffect(
                Require(definition.Stat, nameof(definition.Stat)),
                Require(definition.Layer, nameof(definition.Layer)),
                Require(definition.Value, nameof(definition.Value)),
                source),
            EffectDefinitionType.Revive => new ReviveEffect(Require(definition.Value, nameof(definition.Value))),
            _ => throw new NotSupportedException($"Unknown effect definition type: {definition.Type}."),
        };
    }

    private static T Require<T>(T? value, string fieldName) where T : struct
    {
        if (value is null)
        {
            throw new InvalidOperationException($"Effect definition is missing required field '{fieldName}'.");
        }

        return value.Value;
    }
}
