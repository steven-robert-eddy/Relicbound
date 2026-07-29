using System.Collections.Generic;

namespace Relicbound.Content.Artifacts;

/// <remarks>
/// Raw deserialization target -- deliberately all-nullable so a missing or
/// malformed field is a validation error ArtifactContentLoader reports,
/// rather than a JSON deserialization exception that aborts the whole batch.
/// See src/Relicbound.Content/Schemas/artifact.md for the documented shape.
/// </remarks>
internal sealed class ArtifactJson
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? Tags { get; set; }
    public string? Trigger { get; set; }
    public string? HolderRole { get; set; }
    public string? EffectTarget { get; set; }
    public int? MaxTriggersPerRound { get; set; }
    public string? RequiredTag { get; set; }
    public List<EffectJson>? Effects { get; set; }
}

internal sealed class EffectJson
{
    public string? Type { get; set; }
    public int? Value { get; set; }
    public string? Status { get; set; }
    public int? Stacks { get; set; }
    public int? DurationRounds { get; set; }
    public string? Stat { get; set; }
    public string? Layer { get; set; }
}
