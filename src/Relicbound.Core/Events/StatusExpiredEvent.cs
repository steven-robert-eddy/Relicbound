using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record StatusExpiredEvent(Entity Target, StatusType Status) : IGameEvent
{
    public EventType Type => EventType.StatusExpired;
}
