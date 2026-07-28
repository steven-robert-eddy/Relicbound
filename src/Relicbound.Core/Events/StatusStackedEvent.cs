using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record StatusStackedEvent(Entity Source, Entity Target, StatusType Status, int TotalStacks) : IGameEvent
{
    public EventType Type => EventType.StatusStacked;
}
