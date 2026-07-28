using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;

namespace Relicbound.Core.Effects;

public sealed class ApplyStatusEffect : Effect
{
    public ApplyStatusEffect(StatusType status, int stacks, int durationRounds)
    {
        if (stacks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stacks), "Stacks must be positive.");
        }

        if (durationRounds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationRounds), "Duration must be positive.");
        }

        Status = status;
        Stacks = stacks;
        DurationRounds = durationRounds;
    }

    public StatusType Status { get; }
    public int Stacks { get; }
    public int DurationRounds { get; }

    public override EffectResult Execute(EffectContext context)
    {
        var events = new List<IGameEvent>();

        foreach (var target in context.Targets)
        {
            var statuses = target.Get<Statuses>();
            if (statuses is null)
            {
                statuses = new Statuses();
                target.Add(statuses);
            }

            var stacked = statuses.Add(Status, Stacks, DurationRounds);

            events.Add(stacked
                ? new StatusStackedEvent(context.Source, target, Status, statuses.Get(Status)!.Stacks)
                : new StatusAppliedEvent(context.Source, target, Status, Stacks));
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
