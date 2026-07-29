using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Runes;

namespace Relicbound.Content.Runes;

/// <remarks>
/// Loads once, validates everything, fails loudly with every error at once --
/// same convention as ArtifactContentLoader (docs/TECHNICAL_ARCHITECTURE.md
/// section 10).
/// </remarks>
public static class RuneContentLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<RuneDefinition> LoadEmbedded(Assembly assembly)
    {
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Runes.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal));

        var jsonDocuments = resourceNames.Select(name =>
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });

        return LoadAll(jsonDocuments.ToList());
    }

    public static IReadOnlyList<RuneDefinition> LoadAll(IEnumerable<string> jsonDocuments)
    {
        var definitions = new List<RuneDefinition>();
        var errors = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var json in jsonDocuments)
        {
            RuneJson? dto;
            try
            {
                dto = JsonSerializer.Deserialize<RuneJson>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"Malformed rune JSON: {ex.Message}");
                continue;
            }

            if (dto is null)
            {
                errors.Add("Rune JSON parsed to nothing.");
                continue;
            }

            var runeErrors = new List<string>();
            var definition = TryBuildDefinition(dto, runeErrors);
            var label = dto.Id ?? "<missing id>";

            if (runeErrors.Count > 0)
            {
                errors.AddRange(runeErrors.Select(e => $"[{label}] {e}"));
                continue;
            }

            if (!seenIds.Add(definition!.Id))
            {
                errors.Add($"[{label}] Duplicate rune id.");
                continue;
            }

            definitions.Add(definition);
        }

        if (errors.Count > 0)
        {
            throw new RuneContentLoadException(errors);
        }

        return definitions;
    }

    private static RuneDefinition? TryBuildDefinition(RuneJson dto, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            errors.Add("'id' is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("'name' is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.NamePrefix))
        {
            errors.Add("'namePrefix' is required.");
        }

        var addsTags = new List<Tag>();
        foreach (var rawTag in dto.AddsTags ?? new List<string>())
        {
            if (ContentEnumParsing.TryParse<Tag>(rawTag, out var tag))
            {
                addsTags.Add(tag);
            }
            else
            {
                errors.Add($"'{rawTag}' is not a known tag.");
            }
        }

        var costDelta = dto.CostDelta ?? 0;

        var effects = new List<EffectDefinition>();
        foreach (var effectJson in dto.Effects ?? new List<EffectJson>())
        {
            var effect = EffectDefinitionJsonParsing.TryBuild(effectJson, errors, string.Empty);
            if (effect is not null)
            {
                effects.Add(effect);
            }
        }

        if (dto.ChainAdditionalTargets is not null)
        {
            if (dto.ChainAdditionalTargets is not (> 0))
            {
                errors.Add("'chainAdditionalTargets' must be a positive integer.");
            }

            if (dto.ChainFalloffPercent is not (> 0 and <= 100))
            {
                errors.Add("a rune with 'chainAdditionalTargets' requires 'chainFalloffPercent' between 1 and 100.");
            }
        }
        else if (dto.ChainFalloffPercent is not null)
        {
            errors.Add("'chainFalloffPercent' requires 'chainAdditionalTargets' to also be set.");
        }

        if (errors.Count > 0)
        {
            return null;
        }

        return new RuneDefinition(
            dto.Id!,
            dto.Name!,
            dto.NamePrefix!,
            addsTags,
            costDelta,
            effects,
            dto.ChainAdditionalTargets,
            dto.ChainFalloffPercent);
    }
}
