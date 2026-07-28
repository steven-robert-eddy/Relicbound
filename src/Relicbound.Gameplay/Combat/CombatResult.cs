namespace Relicbound.Gameplay.Combat;

public enum CombatWinner
{
    None,
    Player,
    Enemies
}

public sealed record CombatResult(CombatWinner Winner, int RoundCount);
