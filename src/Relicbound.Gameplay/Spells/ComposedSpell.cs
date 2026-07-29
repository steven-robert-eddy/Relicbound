using System.Collections.Generic;
using Relicbound.Core.Effects;
using Relicbound.Core.Tags;

namespace Relicbound.Gameplay.Spells;

/// <remarks>
/// The result of composing a SpellDefinition with its socketed runes --
/// everything needed to actually cast it. Effects are already compiled to
/// real Core Effects, each carrying the *full* composed Tags, not just
/// whichever fragment (base or a specific rune) contributed it: socketing a
/// Flame Rune makes the whole spell count as Fire, per
/// docs/GAME_DESIGN.md section 7.
/// </remarks>
public sealed record ComposedSpell(
    string Name,
    int Cost,
    IReadOnlyList<Tag> Tags,
    TargetingMode Targeting,
    int? ChainAdditionalTargets,
    int? ChainFalloffPercent,
    IReadOnlyList<Effect> Effects);
