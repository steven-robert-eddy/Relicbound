using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

/// <remarks>
/// Only acts on entities that are actually defeated -- a Phoenix-Feather-like
/// artifact triggering on PLAYER_DEFEATED will only ever see a defeated
/// target, but this guard keeps the effect correct if reused anywhere else.
/// Reuses Health.Heal rather than a new "revive" primitive on Health: Current
/// is already 0 when defeated, so Heal naturally raises it back above zero
/// and IsDefeated (Current &lt;= 0) clears itself.
/// </remarks>
public sealed class ReviveEffect : Effect
{
    public ReviveEffect(int healthPercent, IReadOnlyCollection<Tag>? tags = null)
        : base(tags)
    {
        if (healthPercent is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(healthPercent), "Health percent must be between 1 and 100.");
        }

        HealthPercent = healthPercent;
    }

    public int HealthPercent { get; }

    public override EffectResult Execute(EffectContext context)
    {
        var events = new List<IGameEvent>();

        foreach (var target in context.Targets)
        {
            var health = target.Get<Health>();
            if (health is null || !health.IsDefeated) { continue; }

            var reviveAmount = (health.MaxHealth * HealthPercent + 50) / 100;
            var actual = health.Heal(reviveAmount);
            events.Add(new EntityRevivedEvent(target, actual));
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
