using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Spells;

namespace Relicbound.Content.Spells;

/// <remarks>
/// Loads once, validates everything, fails loudly with every error at once --
/// same convention as ArtifactContentLoader (docs/TECHNICAL_ARCHITECTURE.md
/// section 10).
/// </remarks>
public static class SpellContentLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<SpellDefinition> LoadEmbedded(Assembly assembly)
    {
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Spells.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal));

        var jsonDocuments = resourceNames.Select(name =>
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });

        return LoadAll(jsonDocuments.ToList());
    }

    public static IReadOnlyList<SpellDefinition> LoadAll(IEnumerable<string> jsonDocuments)
    {
        var definitions = new List<SpellDefinition>();
        var errors = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var json in jsonDocuments)
        {
            SpellJson? dto;
            try
            {
                dto = JsonSerializer.Deserialize<SpellJson>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"Malformed spell JSON: {ex.Message}");
                continue;
            }

            if (dto is null)
            {
                errors.Add("Spell JSON parsed to nothing.");
                continue;
            }

            var spellErrors = new List<string>();
            var definition = TryBuildDefinition(dto, spellErrors);
            var label = dto.Id ?? "<missing id>";

            if (spellErrors.Count > 0)
            {
                errors.AddRange(spellErrors.Select(e => $"[{label}] {e}"));
                continue;
            }

            if (!seenIds.Add(definition!.Id))
            {
                errors.Add($"[{label}] Duplicate spell id.");
                continue;
            }

            definitions.Add(definition);
        }

        if (errors.Count > 0)
        {
            throw new SpellContentLoadException(errors);
        }

        return definitions;
    }

    private static SpellDefinition? TryBuildDefinition(SpellJson dto, List<string> errors)
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

        if (dto.Cost is not (> 0))
        {
            errors.Add("'cost' must be a positive integer.");
        }

        if (dto.Range is not (>= 0))
        {
            errors.Add("'range' must be a non-negative integer.");
        }

        if (dto.Sockets is not (>= 0))
        {
            errors.Add("'sockets' must be a non-negative integer.");
        }

        var targeting = TargetingMode.Single;
        if (dto.Targeting is not null && !ContentEnumParsing.TryParse(dto.Targeting, out targeting))
        {
            errors.Add($"'targeting' value '{dto.Targeting}' is not valid.");
        }

        var effects = new List<Relicbound.Gameplay.Artifacts.EffectDefinition>();
        if (dto.Effects is null || dto.Effects.Count == 0)
        {
            errors.Add("'effects' must contain at least one entry.");
        }
        else
        {
            foreach (var effectJson in dto.Effects)
            {
                var effect = EffectDefinitionJsonParsing.TryBuild(effectJson, errors, string.Empty);
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

        return new SpellDefinition(
            dto.Id!,
            dto.Name!,
            tags,
            dto.Cost!.Value,
            dto.Range!.Value,
            dto.Sockets!.Value,
            targeting,
            effects);
    }
}
