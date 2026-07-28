using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

public sealed class DamageEffect : Effect
{
    public DamageEffect(int amount, IReadOnlyCollection<Tag>? tags = null)
        : base(tags)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount cannot be negative.");
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

            health.ApplyDamage(Amount);
            events.Add(new DamageDealtEvent(context.Source, target, Amount));

            if (health.IsDefeated)
            {
                events.Add(new EntityDefeatedEvent(target));

                if (target.Has<PlayerControlled>())
                {
                    events.Add(new PlayerDefeatedEvent(target));
                }
            }
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
