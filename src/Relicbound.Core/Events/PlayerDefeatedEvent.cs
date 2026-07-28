using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record PlayerDefeatedEvent(Entity Target) : IGameEvent
{
    public EventType Type => EventType.PlayerDefeated;
}
