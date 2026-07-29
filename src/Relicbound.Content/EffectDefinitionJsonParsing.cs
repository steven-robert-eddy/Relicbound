using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Stats;
using Relicbound.Gameplay.Artifacts;

namespace Relicbound.Content;

/// <remarks>
/// Shared effect-entry validation for Spells and Runes, mirroring
/// ArtifactContentLoader's own validation rules field for field -- see
/// src/Relicbound.Content/Schemas/artifact.md for the shape this matches.
/// Appends to the caller's error list and returns null on failure, same
/// convention as ArtifactContentLoader, so callers can report every failure
/// in a batch at once rather than stopping at the first.
/// </remarks>
public static class EffectDefinitionJsonParsing
{
    public static EffectDefinition? TryBuild(EffectJson effectJson, List<string> errors, string contextLabel)
    {
        if (!ContentEnumParsing.TryParse<EffectDefinitionType>(effectJson.Type, out var type))
        {
            errors.Add($"{contextLabel} effect 'type' value '{effectJson.Type}' is not a known effect type.");
            return null;
        }

        switch (type)
        {
            case EffectDefinitionType.Damage:
            case EffectDefinitionType.Heal:
            case EffectDefinitionType.Shield:
                if (effectJson.Value is not (> 0))
                {
                    errors.Add($"{contextLabel} a {type} effect requires a positive 'value'.");
                    return null;
                }

                return new EffectDefinition(type, Value: effectJson.Value);

            case EffectDefinitionType.Revive:
                if (effectJson.Value is not (> 0 and <= 100))
                {
                    errors.Add($"{contextLabel} a REVIVE effect requires a 'value' between 1 and 100 (percent of max health).");
                    return null;
                }

                return new EffectDefinition(type, Value: effectJson.Value);

            case EffectDefinitionType.ApplyStatus:
                if (!ContentEnumParsing.TryParse<StatusType>(effectJson.Status, out var status))
                {
                    errors.Add($"{contextLabel} an APPLY_STATUS effect has an invalid or missing 'status': '{effectJson.Status}'.");
                    return null;
                }

                if (effectJson.Stacks is not (> 0))
                {
                    errors.Add($"{contextLabel} an APPLY_STATUS effect requires a positive 'stacks'.");
                    return null;
                }

                if (effectJson.DurationRounds is not (> 0))
                {
                    errors.Add($"{contextLabel} an APPLY_STATUS effect requires a positive 'durationRounds'.");
                    return null;
                }

                return new EffectDefinition(
                    type, Status: status, Stacks: effectJson.Stacks, DurationRounds: effectJson.DurationRounds);

            case EffectDefinitionType.ModifyStat:
                if (!ContentEnumParsing.TryParse<StatType>(effectJson.Stat, out var stat))
                {
                    errors.Add($"{contextLabel} a MODIFY_STAT effect has an invalid or missing 'stat': '{effectJson.Stat}'.");
                    return null;
                }

                if (!ContentEnumParsing.TryParse<ModifierLayer>(effectJson.Layer, out var layer))
                {
                    errors.Add($"{contextLabel} a MODIFY_STAT effect has an invalid or missing 'layer': '{effectJson.Layer}'.");
                    return null;
                }

                if (effectJson.Value is null)
                {
                    errors.Add($"{contextLabel} a MODIFY_STAT effect requires a 'value'.");
                    return null;
                }

                return new EffectDefinition(type, Value: effectJson.Value, Stat: stat, Layer: layer);

            default:
                errors.Add($"{contextLabel} effect type '{type}' has no known validation rule.");
                return null;
        }
    }
}
