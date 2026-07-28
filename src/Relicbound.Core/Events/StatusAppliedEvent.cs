using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record StatusAppliedEvent(Entity Source, Entity Target, StatusType Status, int Stacks) : IGameEvent
{
    public EventType Type => EventType.StatusApplied;
}
