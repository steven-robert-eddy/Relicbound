using System.Collections.Generic;
using Relicbound.Core.Events;

namespace Relicbound.Core.Effects;

/// <remarks>
/// The seam Milestone 2's trigger registry plugs into. Core knows only that
/// "something may react to an event with more effects" -- it has no notion
/// of artifacts, equipment, or tags. Implementations decide ordering and
/// per-round budgets (docs/TECHNICAL_ARCHITECTURE.md section 7); the depth
/// cap and step ceiling remain EffectResolver's job regardless of what a
/// trigger source returns.
/// </remarks>
public interface ITriggerSource
{
    IReadOnlyList<TriggerActivation> Match(IGameEvent gameEvent);
}
