using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Stats;

namespace Relicbound.Core.Effects;

public sealed class ModifyStatEffect : Effect
{
    public ModifyStatEffect(StatType stat, ModifierLayer layer, int value, ModifierSource source)
    {
        Stat = stat;
        Layer = layer;
        Value = value;
        Source = source;
    }

    public StatType Stat { get; }
    public ModifierLayer Layer { get; }
    public int Value { get; }
    public ModifierSource Source { get; }

    public override EffectResult Execute(EffectContext context)
    {
        var events = new List<IGameEvent>();

        foreach (var target in context.Targets)
        {
            var stats = target.Get<StatBlock>();
            if (stats is null)
            {
                stats = new StatBlock();
                target.Add(stats);
            }

            stats.AddModifier(new StatModifier(Stat, Layer, Value, Source));
            events.Add(new StatModifiedEvent(target, Stat, Layer, Value));
        }

        return new EffectResult(events, Array.Empty<QueuedEffect>());
    }
}
