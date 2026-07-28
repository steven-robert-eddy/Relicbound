using Relicbound.Core.Entities;

namespace Relicbound.Core.Events;

/// <remarks>
/// Announces that an artifact fired, independent of whatever its effects go
/// on to do. Core has no notion of artifacts beyond an opaque id -- the
/// trigger registry (Relicbound.Gameplay.Artifacts) is what actually knows
/// what "ember_heart" means.
/// </remarks>
public sealed record ArtifactTriggeredEvent(Entity Holder, string ArtifactId) : IGameEvent
{
    public EventType Type => EventType.ArtifactTriggered;
}
