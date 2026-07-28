using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record EntityDefeatedEvent(Entity Target) : IGameEvent
{
    public EventType Type => EventType.EntityDefeated;
}
