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
/// This owns two of the four guards described there directly -- depth cap
/// and step ceiling. The other two (per-round trigger budget, deterministic
/// trigger ordering) are the responsibility of whatever <see cref="ITriggerSource"/>
/// is supplied; Core has no notion of artifacts or equipment to order or
/// budget by.
/// </remarks>
public sealed class EffectResolver
{
    public const int MaxTriggerDepth = 8;
    public const int MaxResolutionSteps = 512;

    private readonly IEventBus _eventBus;
    private readonly Journal _journal;
    private readonly ITriggerSource? _triggerSource;

    public EffectResolver(IEventBus eventBus, Journal journal, ITriggerSource? triggerSource = null)
    {
        _eventBus = eventBus;
        _journal = journal;
        _triggerSource = triggerSource;
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

                if (_triggerSource is null || currentContext.Depth >= MaxTriggerDepth)
                {
                    continue;
                }

                foreach (var activation in _triggerSource.Match(gameEvent))
                {
                    _journal.Record(activation.AnnouncementEvent);
                    _eventBus.Publish(activation.AnnouncementEvent);

                    foreach (var triggered in activation.Effects)
                    {
                        var triggeredContext = new EffectContext(
                            currentContext.Source,
                            triggered.Targets,
                            currentContext.Random,
                            currentContext.Depth + 1,
                            EffectOrigin.Artifact);

                        queue.Enqueue((triggered.Effect, triggeredContext));
                    }
                }
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
