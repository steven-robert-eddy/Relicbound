using System;
using System.Collections.Generic;
using Relicbound.Core.Effects;
using Relicbound.Core.Stats;
using Relicbound.Core.Tags;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Compiles data into the real, generic Core effects -- the bridge between
/// "content is JSON" and "gameplay changes are Effects". Throwing on a
/// missing field here is a last-resort safety net; ArtifactContentLoader is
/// expected to have already validated every field this needs.
///
/// The optional <paramref name="tags"/> parameter lets a caller (SpellComposer,
/// for a socketed spell) stamp the composed spell's full tag set onto every
/// compiled effect, not just whichever fragment contributed it -- see
/// docs/GAME_DESIGN.md section 7. Existing callers that omit it are
/// unaffected; effects default to no tags, same as before.
/// </remarks>
public static class EffectDefinitionFactory
{
    public static Effect Create(EffectDefinition definition, ModifierSource source, IReadOnlyCollection<Tag>? tags = null)
    {
        return definition.Type switch
        {
            EffectDefinitionType.Damage => new DamageEffect(Require(definition.Value, nameof(definition.Value)), tags),
            EffectDefinitionType.Heal => new HealEffect(Require(definition.Value, nameof(definition.Value)), tags),
            EffectDefinitionType.ApplyStatus => new ApplyStatusEffect(
                Require(definition.Status, nameof(definition.Status)),
                Require(definition.Stacks, nameof(definition.Stacks)),
                Require(definition.DurationRounds, nameof(definition.DurationRounds)),
                tags),
            EffectDefinitionType.ModifyStat => new ModifyStatEffect(
                Require(definition.Stat, nameof(definition.Stat)),
                Require(definition.Layer, nameof(definition.Layer)),
                Require(definition.Value, nameof(definition.Value)),
                source,
                tags),
            EffectDefinitionType.Revive => new ReviveEffect(Require(definition.Value, nameof(definition.Value)), tags),
            EffectDefinitionType.Shield => new ShieldEffect(Require(definition.Value, nameof(definition.Value)), tags),
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
