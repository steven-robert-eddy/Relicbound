using System.Collections.Generic;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;

namespace Relicbound.Gameplay.Spells;

/// <remarks>
/// A base spell: pure data, per docs/GAME_DESIGN.md section 7. Sockets is
/// just a count -- SpellComposer resolves which runes are actually
/// socketed; the base definition doesn't know or care which, or how many.
/// </remarks>
public sealed record SpellDefinition(
    string Id,
    string Name,
    IReadOnlyList<Tag> Tags,
    int Cost,
    int Range,
    int Sockets,
    TargetingMode Targeting,
    IReadOnlyList<EffectDefinition> Effects);
