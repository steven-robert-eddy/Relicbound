using System.Collections.Generic;

namespace Relicbound.Gameplay.Encounters;

/// <remarks>
/// Pure data, per docs/CODING_STANDARDS.md -- an encounter is enemy
/// composition plus a difficulty tier, nothing else yet. Grid layout isn't
/// here: no content currently varies it, so a field with one always-used
/// default would be unearned complexity until something needs it.
/// </remarks>
public sealed record EncounterDefinition(
    string Id,
    string Name,
    DifficultyTier DifficultyTier,
    IReadOnlyList<EncounterEnemy> Enemies);
