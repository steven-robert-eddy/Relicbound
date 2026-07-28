using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

public sealed record ArtifactUnequippedEvent(Entity Holder, string ArtifactId) : IGameEvent
{
    public EventType Type => EventType.ArtifactUnequipped;
}
