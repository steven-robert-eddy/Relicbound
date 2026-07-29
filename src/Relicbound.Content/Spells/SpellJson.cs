using System.Collections.Generic;

namespace Relicbound.Content.Spells;

/// <remarks>
/// Raw deserialization target -- deliberately all-nullable so a missing or
/// malformed field is a validation error SpellContentLoader reports, rather
/// than a JSON deserialization exception that aborts the whole batch. See
/// src/Relicbound.Content/Schemas/spell.md for the documented shape.
/// </remarks>
internal sealed class SpellJson
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? Tags { get; set; }
    public int? Cost { get; set; }
    public int? Range { get; set; }
    public int? Sockets { get; set; }
    public string? Targeting { get; set; }
    public List<EffectJson>? Effects { get; set; }
}
