using System;
using System.Collections.Generic;
using Relicbound.Core.Events;

namespace Relicbound.Core.Effects;

public sealed class EffectResult
{
    public static readonly EffectResult Empty = new(Array.Empty<IGameEvent>(), Array.Empty<QueuedEffect>());

    public EffectResult(IReadOnlyList<IGameEvent> events, IReadOnlyList<QueuedEffect> queued)
    {
        Events = events;
        Queued = queued;
    }

    public IReadOnlyList<IGameEvent> Events { get; }
    public IReadOnlyList<QueuedEffect> Queued { get; }
}
