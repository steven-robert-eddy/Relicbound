using System.Collections.Generic;

namespace Relicbound.Content.Runes;

/// <remarks>
/// Raw deserialization target -- deliberately all-nullable so a missing or
/// malformed field is a validation error RuneContentLoader reports, rather
/// than a JSON deserialization exception that aborts the whole batch. See
/// src/Relicbound.Content/Schemas/rune.md for the documented shape.
/// </remarks>
internal sealed class RuneJson
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? NamePrefix { get; set; }
    public List<string>? AddsTags { get; set; }
    public int? CostDelta { get; set; }
    public List<EffectJson>? Effects { get; set; }
    public int? ChainAdditionalTargets { get; set; }
    public int? ChainFalloffPercent { get; set; }
}
