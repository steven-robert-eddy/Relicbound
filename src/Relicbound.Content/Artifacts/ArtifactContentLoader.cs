using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Stats;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;

namespace Relicbound.Content.Artifacts;

/// <remarks>
/// Loads once, validates everything, fails loudly with every error at once --
/// per docs/TECHNICAL_ARCHITECTURE.md section 10. A malformed or invalid
/// artifact is a content bug to fix, not a runtime condition to recover from,
/// so a non-empty error list always throws rather than returning a partial
/// result.
/// </remarks>
public static class ArtifactContentLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<ArtifactDefinition> LoadAll(IEnumerable<string> jsonDocuments)
    {
        var definitions = new List<ArtifactDefinition>();
        var errors = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var json in jsonDocuments)
        {
            ArtifactJson? dto;
            try
            {
                dto = JsonSerializer.Deserialize<ArtifactJson>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"Malformed artifact JSON: {ex.Message}");
                continue;
            }

            if (dto is null)
            {
                errors.Add("Artifact JSON parsed to nothing.");
                continue;
            }

            var artifactErrors = new List<string>();
            var definition = TryBuildDefinition(dto, artifactErrors);
            var label = dto.Id ?? "<missing id>";

            if (artifactErrors.Count > 0)
            {
                errors.AddRange(artifactErrors.Select(e => $"[{label}] {e}"));
                continue;
            }

            if (!seenIds.Add(definition!.Id))
            {
                errors.Add($"[{label}] Duplicate artifact id.");
                continue;
            }

            definitions.Add(definition);
        }

        if (errors.Count > 0)
        {
            throw new ArtifactContentLoadException(errors);
        }

        return definitions;
    }

    private static ArtifactDefinition? TryBuildDefinition(ArtifactJson dto, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            errors.Add("'id' is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("'name' is required.");
        }

        var tags = new List<Tag>();
        foreach (var rawTag in dto.Tags ?? new List<string>())
        {
            if (ContentEnumParsing.TryParse<Tag>(rawTag, out var tag))
            {
                tags.Add(tag);
            }
            else
            {
                errors.Add($"'{rawTag}' is not a known tag.");
            }
        }

        if (!ContentEnumParsing.TryParse<EventType>(dto.Trigger, out var trigger))
        {
            errors.Add($"'trigger' value '{dto.Trigger}' is not a known event type.");
        }

        var holderRole = TriggerHolderRole.Actor;
        if (dto.HolderRole is not null && !ContentEnumParsing.TryParse(dto.HolderRole, out holderRole))
        {
            errors.Add($"'holderRole' value '{dto.HolderRole}' is not valid.");
        }

        var effectTarget = EffectTargetSelector.EventRecipient;
        if (dto.EffectTarget is not null && !ContentEnumParsing.TryParse(dto.EffectTarget, out effectTarget))
        {
            errors.Add($"'effectTarget' value '{dto.EffectTarget}' is not valid.");
        }

        var maxTriggersPerRound = dto.MaxTriggersPerRound ?? 1;
        if (maxTriggersPerRound <= 0)
        {
            errors.Add("'maxTriggersPerRound' must be positive.");
        }

        var effects = new List<EffectDefinition>();
        if (dto.Effects is null || dto.Effects.Count == 0)
        {
            errors.Add("'effects' must contain at least one entry.");
        }
        else
        {
            foreach (var effectJson in dto.Effects)
            {
                var effect = TryBuildEffect(effectJson, errors);
                if (effect is not null)
                {
                    effects.Add(effect);
                }
            }
        }

        if (errors.Count > 0)
        {
            return null;
        }

        return new ArtifactDefinition(
            dto.Id!,
            dto.Name!,
            tags,
            trigger,
            effects,
            holderRole,
            effectTarget,
            maxTriggersPerRound);
    }

    private static EffectDefinition? TryBuildEffect(EffectJson effectJson, List<string> errors)
    {
        if (!ContentEnumParsing.TryParse<EffectDefinitionType>(effectJson.Type, out var type))
        {
            errors.Add($"effect 'type' value '{effectJson.Type}' is not a known effect type.");
            return null;
        }

        switch (type)
        {
            case EffectDefinitionType.Damage:
            case EffectDefinitionType.Heal:
                if (effectJson.Value is not (> 0))
                {
                    errors.Add($"a {type} effect requires a positive 'value'.");
                    return null;
                }

                return new EffectDefinition(type, Value: effectJson.Value);

            case EffectDefinitionType.ApplyStatus:
                if (!ContentEnumParsing.TryParse<StatusType>(effectJson.Status, out var status))
                {
                    errors.Add($"an APPLY_STATUS effect has an invalid or missing 'status': '{effectJson.Status}'.");
                    return null;
                }

                if (effectJson.Stacks is not (> 0))
                {
                    errors.Add("an APPLY_STATUS effect requires a positive 'stacks'.");
                    return null;
                }

                if (effectJson.DurationRounds is not (> 0))
                {
                    errors.Add("an APPLY_STATUS effect requires a positive 'durationRounds'.");
                    return null;
                }

                return new EffectDefinition(
                    type, Status: status, Stacks: effectJson.Stacks, DurationRounds: effectJson.DurationRounds);

            case EffectDefinitionType.ModifyStat:
                if (!ContentEnumParsing.TryParse<StatType>(effectJson.Stat, out var stat))
                {
                    errors.Add($"a MODIFY_STAT effect has an invalid or missing 'stat': '{effectJson.Stat}'.");
                    return null;
                }

                if (!ContentEnumParsing.TryParse<ModifierLayer>(effectJson.Layer, out var layer))
                {
                    errors.Add($"a MODIFY_STAT effect has an invalid or missing 'layer': '{effectJson.Layer}'.");
                    return null;
                }

                if (effectJson.Value is null)
                {
                    errors.Add("a MODIFY_STAT effect requires a 'value'.");
                    return null;
                }

                return new EffectDefinition(type, Value: effectJson.Value, Stat: stat, Layer: layer);

            default:
                errors.Add($"effect type '{type}' has no known validation rule.");
                return null;
        }
    }
}
