using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record DamageDealtEvent(Entity Source, Entity Target, int Amount) : IGameEvent
{
    public EventType Type => EventType.DamageDealt;
}
