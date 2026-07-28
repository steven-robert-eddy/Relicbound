using Relicbound.Core.Entities;
using Relicbound.Core.Stats;

namespace Relicbound.Core.Events;

public sealed record StatModifiedEvent(Entity Target, StatType Stat, ModifierLayer Layer, int Value) : IGameEvent
{
    public EventType Type => EventType.StatModified;
}
