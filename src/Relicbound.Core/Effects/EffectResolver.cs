using System;
using System.Collections.Generic;
using Relicbound.Core.Events;

namespace Relicbound.Core.Effects;

/// <remarks>
/// Resolves effects iteratively rather than recursively, so a chain of
/// effects queuing further effects cannot overflow the stack, and is capped
/// by depth and a total step ceiling so a cycle terminates instead of
/// hanging. See docs/TECHNICAL_ARCHITECTURE.md section 7.
///
/// This covers two of the four guards described there — depth cap and step
/// ceiling. The other two (per-round trigger budget, deterministic trigger
/// ordering) attach once Milestone 2's trigger registry decides what gets
/// enqueued in response to an event; there is nothing to budget or order
/// until something external is triggering off events.
/// </remarks>
public sealed class EffectResolver
{
    public const int MaxTriggerDepth = 8;
    public const int MaxResolutionSteps = 512;

    private readonly IEventBus _eventBus;
    private readonly Journal _journal;

    public EffectResolver(IEventBus eventBus, Journal journal)
    {
        _eventBus = eventBus;
        _journal = journal;
    }

    public void Resolve(Effect effect, EffectContext context)
    {
        var queue = new Queue<(Effect Effect, EffectContext Context)>();
        queue.Enqueue((effect, context));

        var steps = 0;

        while (queue.Count > 0)
        {
            if (steps++ >= MaxResolutionSteps)
            {
                throw new InvalidOperationException(
                    $"Effect resolution exceeded {MaxResolutionSteps} steps. "
                        + "This is a content bug, not a runtime condition to recover from.");
            }

            var (currentEffect, currentContext) = queue.Dequeue();
            var result = currentEffect.Execute(currentContext);

            foreach (var gameEvent in result.Events)
            {
                _journal.Record(gameEvent);
                _eventBus.Publish(gameEvent);
            }

            foreach (var queued in result.Queued)
            {
                if (currentContext.Depth >= MaxTriggerDepth)
                {
                    continue;
                }

                var nextContext = new EffectContext(
                    currentContext.Source,
                    queued.Targets,
                    currentContext.Random,
                    currentContext.Depth + 1,
                    currentContext.Origin);

                queue.Enqueue((queued.Effect, nextContext));
            }
        }
    }
}
