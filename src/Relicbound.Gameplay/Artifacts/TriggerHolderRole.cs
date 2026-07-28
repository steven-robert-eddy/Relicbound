namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Which role the artifact's holder must play in the triggering event for it
/// to fire. Ember Heart only procs off damage its own holder dealt (Actor);
/// Phoenix Feather only procs off its own holder's defeat (Recipient).
/// </remarks>
public enum TriggerHolderRole
{
    Actor,
    Recipient
}
