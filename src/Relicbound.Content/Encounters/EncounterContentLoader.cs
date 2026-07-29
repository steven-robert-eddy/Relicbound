using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Relicbound.Gameplay.Encounters;

namespace Relicbound.Content.Encounters;

/// <remarks>
/// Loads once, validates everything, fails loudly with every error at once --
/// same convention as ArtifactContentLoader (docs/TECHNICAL_ARCHITECTURE.md
/// section 10).
/// </remarks>
public static class EncounterContentLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<EncounterDefinition> LoadEmbedded(Assembly assembly)
    {
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Encounters.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal));

        var jsonDocuments = resourceNames.Select(name =>
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });

        return LoadAll(jsonDocuments.ToList());
    }

    public static IReadOnlyList<EncounterDefinition> LoadAll(IEnumerable<string> jsonDocuments)
    {
        var definitions = new List<EncounterDefinition>();
        var errors = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var json in jsonDocuments)
        {
            EncounterJson? dto;
            try
            {
                dto = JsonSerializer.Deserialize<EncounterJson>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"Malformed encounter JSON: {ex.Message}");
                continue;
            }

            if (dto is null)
            {
                errors.Add("Encounter JSON parsed to nothing.");
                continue;
            }

            var encounterErrors = new List<string>();
            var definition = TryBuildDefinition(dto, encounterErrors);
            var label = dto.Id ?? "<missing id>";

            if (encounterErrors.Count > 0)
            {
                errors.AddRange(encounterErrors.Select(e => $"[{label}] {e}"));
                continue;
            }

            if (!seenIds.Add(definition!.Id))
            {
                errors.Add($"[{label}] Duplicate encounter id.");
                continue;
            }

            definitions.Add(definition);
        }

        if (errors.Count > 0)
        {
            throw new EncounterContentLoadException(errors);
        }

        return definitions;
    }

    private static EncounterDefinition? TryBuildDefinition(EncounterJson dto, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            errors.Add("'id' is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("'name' is required.");
        }

        var difficultyTier = DifficultyTier.Standard;
        if (!ContentEnumParsing.TryParse(dto.DifficultyTier, out difficultyTier))
        {
            errors.Add($"'difficultyTier' value '{dto.DifficultyTier}' is not valid.");
        }

        var enemies = new List<EncounterEnemy>();
        if (dto.Enemies is null || dto.Enemies.Count == 0)
        {
            errors.Add("'enemies' must contain at least one entry.");
        }
        else
        {
            foreach (var enemyJson in dto.Enemies)
            {
                if (string.IsNullOrWhiteSpace(enemyJson.Id))
                {
                    errors.Add("an enemy's 'id' is required.");
                }

                if (string.IsNullOrWhiteSpace(enemyJson.Name))
                {
                    errors.Add("an enemy's 'name' is required.");
                }

                if (enemyJson.Health is not (> 0))
                {
                    errors.Add($"enemy '{enemyJson.Id ?? "<missing id>"}': 'health' must be a positive integer.");
                }

                if (!string.IsNullOrWhiteSpace(enemyJson.Id) && !string.IsNullOrWhiteSpace(enemyJson.Name) && enemyJson.Health is > 0)
                {
                    enemies.Add(new EncounterEnemy(enemyJson.Id!, enemyJson.Name!, enemyJson.Health!.Value));
                }
            }
        }

        if (errors.Count > 0)
        {
            return null;
        }

        return new EncounterDefinition(dto.Id!, dto.Name!, difficultyTier, enemies);
    }
}
