using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

public sealed class HealEffect : Effect
{
    public HealEffect(int amount, IReadOnlyCollection<Tag>? tags = null)
        : base(tags)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative.");
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

            var actual = health.Heal(Amount);
            if (actual > 0)
            {
                events.Add(new HealedEvent(context.Source, target, actual));
            }
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
