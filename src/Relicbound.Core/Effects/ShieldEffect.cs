using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

public sealed class ShieldEffect : Effect
{
    public ShieldEffect(int amount, IReadOnlyCollection<Tag>? tags = null)
        : base(tags)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Shield amount must be positive.");
        }

        Amount = amount;
    }

    public int Amount { get; }

    public override EffectResult Execute(EffectContext context)
    {
        var events = new List<IGameEvent>();

        foreach (var target in context.Targets)
        {
            var health = target.Get<Health>();
            if (health is null) { continue; }

            health.AddShield(Amount);
            events.Add(new ShieldGainedEvent(target, Amount));
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
