using System.Collections.Generic;

namespace Relicbound.Content.Encounters;

/// <remarks>
/// Raw deserialization target -- deliberately all-nullable so a missing or
/// malformed field is a validation error EncounterContentLoader reports,
/// rather than a JSON deserialization exception that aborts the whole batch.
/// See src/Relicbound.Content/Schemas/encounter.md for the documented shape.
/// </remarks>
internal sealed class EncounterJson
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? DifficultyTier { get; set; }
    public List<EncounterEnemyJson>? Enemies { get; set; }
}

internal sealed class EncounterEnemyJson
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Health { get; set; }
}
