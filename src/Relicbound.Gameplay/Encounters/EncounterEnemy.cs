namespace Relicbound.Gameplay.Encounters;

/// <remarks>
/// Just enough to stand an enemy up in CombatSetup -- Id, Name, Health. No
/// stats, abilities, or AI pattern of its own yet; there's no enemy content
/// system to draw those from (src/Relicbound.Content/Enemies is still a
/// placeholder). Every enemy in the prototype behaves like the sandbox's
/// goblin (EnemyIntentPlanner: approach and attack).
/// </remarks>
public sealed record EncounterEnemy(string Id, string Name, int Health);
