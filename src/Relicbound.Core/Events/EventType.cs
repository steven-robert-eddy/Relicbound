namespace Relicbound.Core.Events;

// Initial subset for Milestone 1 (damage, healing, defeat). The full
// catalogue in docs/TECHNICAL_ARCHITECTURE.md section 5 grows here as later
// milestones add turns, statuses, and artifact triggers.
public enum EventType
{
    DamageDealt,
    Healed,
    EntityDefeated,
    PlayerDefeated
}
