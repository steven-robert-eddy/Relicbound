using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record IntentDeclaredEvent(Entity Source, Intent Intent) : IGameEvent
{
    public EventType Type => EventType.IntentDeclared;
}
