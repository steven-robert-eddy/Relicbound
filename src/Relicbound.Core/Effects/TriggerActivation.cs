using System.Collections.Generic;
using Relicbound.Core.Events;

namespace Relicbound.Core.Effects;

/// <remarks>
/// One firing of one trigger: the event that announces it fired (journaled
/// so it shows up in the combat log, per docs/PROTOTYPE_ROADMAP.md's
/// Milestone 2 success criteria) plus the effects it queues. A single
/// artifact firing with three listed effects is one activation with three
/// queued effects, not three separate announcements.
/// </remarks>
public sealed record TriggerActivation(IGameEvent AnnouncementEvent, IReadOnlyList<QueuedEffect> Effects);
