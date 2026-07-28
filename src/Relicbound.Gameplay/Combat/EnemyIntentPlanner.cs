using Relicbound.Core.Entities;
using Relicbound.Core.Rules;

namespace Relicbound.Gameplay.Combat;

/// <remarks>
/// Minimal Milestone 1 behaviour: attack if already adjacent to the target,
/// otherwise take one step closer. This is what the enemy "decides" at the
/// start of its turn — the Intent it produces is what makes that decision
/// visible to the player before it resolves.
/// </remarks>
public static class EnemyIntentPlanner
{
    public static void Plan(Entity actor, Entity target, Grid grid)
    {
        var actorPosition = actor.Get<GridPosition>();
        var targetPosition = target.Get<GridPosition>();
        var intent = actor.Get<Intent>();

        if (actorPosition is null || targetPosition is null || intent is null)
        {
            return;
        }

        if (actorPosition.Point.ManhattanDistance(targetPosition.Point) <= 1)
        {
            intent.SetAttack(target.Id);
            return;
        }

        var step = StepToward(actorPosition.Point, targetPosition.Point, grid);
        intent.SetMove(step);
    }

    private static GridPoint StepToward(GridPoint from, GridPoint to, Grid grid)
    {
        var best = from;
        var bestDistance = from.ManhattanDistance(to);

        foreach (var neighbor in grid.Neighbors(from))
        {
            var distance = neighbor.ManhattanDistance(to);
            if (distance < bestDistance)
            {
                best = neighbor;
                bestDistance = distance;
            }
        }

        return best;
    }
}
