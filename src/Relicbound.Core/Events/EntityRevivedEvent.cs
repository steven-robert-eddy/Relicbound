using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record EntityRevivedEvent(Entity Target, int Amount) : IGameEvent
{
    public EventType Type => EventType.EntityRevived;
}
