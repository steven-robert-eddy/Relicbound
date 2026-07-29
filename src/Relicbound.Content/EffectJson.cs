namespace Relicbound.Content;

/// <remarks>
/// Same shape as Artifacts' own internal EffectJson -- shared here so
/// SpellContentLoader and RuneContentLoader don't each duplicate the effect
/// validation rules. Unifying this with Artifacts' copy too is a worthwhile
/// follow-up, not done here to avoid touching that file for this pass.
/// </remarks>
public sealed class EffectJson
{
    public string? Type { get; set; }
    public int? Value { get; set; }
    public string? Status { get; set; }
    public int? Stacks { get; set; }
    public int? DurationRounds { get; set; }
    public string? Stat { get; set; }
    public string? Layer { get; set; }
}
