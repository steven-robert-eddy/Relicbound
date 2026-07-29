using System.Collections.Generic;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Pure data: tags + trigger + effects, per docs/GAME_DESIGN.md section 6. No
/// artifact-specific code exists anywhere else -- Ember Heart and Phoenix
/// Feather are both just instances of this record.
///
/// RequiredTag is optional and defaults to null, meaning "fires on any event
/// of the right type" -- Ember Heart and Phoenix Feather's existing
/// unconditional behavior is unchanged. Set it to gate a trigger on the
/// firing effect also carrying a specific tag, e.g. an artifact that only
/// reacts to Fire damage: proves that "socketing a Flame Rune makes the
/// spell count as Fire" (docs/GAME_DESIGN.md section 7) actually does
/// something, not just carries the label.
/// </remarks>
public sealed record ArtifactDefinition(
    string Id,
    string Name,
    IReadOnlyList<Tag> Tags,
    EventType Trigger,
    IReadOnlyList<EffectDefinition> Effects,
    TriggerHolderRole HolderRole = TriggerHolderRole.Actor,
    EffectTargetSelector EffectTarget = EffectTargetSelector.EventRecipient,
    int MaxTriggersPerRound = 1,
    Tag? RequiredTag = null);
