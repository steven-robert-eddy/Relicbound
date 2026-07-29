using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record ShieldGainedEvent(Entity Target, int Amount) : IGameEvent
{
    public EventType Type => EventType.ShieldGained;
}
