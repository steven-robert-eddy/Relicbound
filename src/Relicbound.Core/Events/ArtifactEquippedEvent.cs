using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record ArtifactEquippedEvent(Entity Holder, string ArtifactId) : IGameEvent
{
    public EventType Type => EventType.ArtifactEquipped;
}
