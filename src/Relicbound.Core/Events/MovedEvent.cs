using Relicbound.Core.Entities;
using Relicbound.Core.Rules;

namespace Relicbound.Core.Events;

public sealed record MovedEvent(Entity Target, GridPoint From, GridPoint To) : IGameEvent
{
    public EventType Type => EventType.Moved;
}
