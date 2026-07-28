using System.Collections.Generic;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Pure data: tags + trigger + effects, per docs/GAME_DESIGN.md section 6. No
/// artifact-specific code exists anywhere else -- Ember Heart and Phoenix
/// Feather are both just instances of this record.
/// </remarks>
public sealed record ArtifactDefinition(
    string Id,
    string Name,
    IReadOnlyList<Tag> Tags,
    EventType Trigger,
    IReadOnlyList<EffectDefinition> Effects,
    TriggerHolderRole HolderRole = TriggerHolderRole.Actor,
    EffectTargetSelector EffectTarget = EffectTargetSelector.EventRecipient,
    int MaxTriggersPerRound = 1);
