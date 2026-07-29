using System.Collections.Generic;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

/// <remarks>
/// The seam Milestone 2's trigger registry plugs into. Core knows only that
/// "something may react to an event with more effects" -- it has no notion
/// of artifacts or equipment. It does hand over the firing effect's own
/// Tags (a Core concept, not an artifact one) so an implementation can gate
/// on them if it wants to -- see docs/GAME_DESIGN.md section 7 ("socketing a
/// Flame Rune makes that spell count as Fire"). Implementations decide
/// ordering and per-round budgets (docs/TECHNICAL_ARCHITECTURE.md section
/// 7); the depth cap and step ceiling remain EffectResolver's job
/// regardless of what a trigger source returns.
/// </remarks>
public interface ITriggerSource
{
    IReadOnlyList<TriggerActivation> Match(IGameEvent gameEvent, IReadOnlyCollection<Tag>? effectTags = null);
}
