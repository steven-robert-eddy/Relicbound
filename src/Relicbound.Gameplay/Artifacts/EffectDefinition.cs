using Relicbound.Core.Entities;
using Relicbound.Core.Stats;


namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// One entry from an artifact's "effects" array. Fields are nullable because
/// only the subset relevant to <see cref="Type"/> is populated -- see
/// EffectDefinitionFactory for which fields each kind requires, and
/// src/Relicbound.Content/Schemas for the JSON shape.
/// </remarks>
public sealed record EffectDefinition(
    EffectDefinitionType Type,
    int? Value = null,
    StatusType? Status = null,
    int? Stacks = null,
    int? DurationRounds = null,
    StatType? Stat = null,
    ModifierLayer? Layer = null);
