namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Which entity an artifact's effects apply to once it has triggered.
/// </remarks>
public enum EffectTargetSelector
{
    /// <summary>The artifact's own holder.</summary>
    Self,

    /// <summary>Whoever acted in the triggering event (e.g. the attacker).</summary>
    EventActor,

    /// <summary>Whoever was acted upon (e.g. the entity damaged or defeated).</summary>
    EventRecipient
}
