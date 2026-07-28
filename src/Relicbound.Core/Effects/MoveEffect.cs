using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

/// <remarks>
/// Kept dumb on purpose: this only moves whatever it's given. Bounds,
/// occupancy, and AP-affordability are simulation-level legality concerns,
/// checked by the caller before this effect is ever resolved.
/// </remarks>
public sealed class MoveEffect : Effect
{
    public MoveEffect(GridPoint destination, IReadOnlyCollection<Tag>? tags = null)
        : base(tags)
    {
        Destination = destination;
    }

    public GridPoint Destination { get; }

    public override EffectResult Execute(EffectContext context)
    {
        var events = new List<IGameEvent>();

        foreach (var target in context.Targets)
        {
            var position = target.Get<GridPosition>();
            if (position is null) { continue; }

            var origin = position.Point;
            position.MoveTo(Destination);
            events.Add(new MovedEvent(target, origin, Destination));
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
