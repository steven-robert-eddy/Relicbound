using Relicbound.Core.Entities;
using Relicbound.Core.Rules;
using Relicbound.Gameplay.Combat;
using Xunit;

namespace Relicbound.Gameplay.Tests.Combat;

public class EnemyIntentPlannerTests
{
    private static Entity CreateEntityAt(int id, string name, int x, int y)
    {
        var entity = new Entity(new EntityId(id), name);
        entity.Add(new GridPosition(new GridPoint(x, y)));
        entity.Add(new Intent());
        return entity;
    }

    [Fact]
    public void Plan_WhenAdjacentToTarget_SetsAttackIntent()
    {
        var grid = new Grid(9, 7);
        var enemy = CreateEntityAt(1, "Goblin", 3, 3);
        var player = CreateEntityAt(2, "Player", 4, 3);

        EnemyIntentPlanner.Plan(enemy, player, grid);

        var intent = enemy.Get<Intent>()!;
        Assert.Equal(IntentKind.Attack, intent.Kind);
        Assert.Equal(player.Id, intent.TargetEntityId);
    }

    [Fact]
    public void Plan_WhenFarFromTarget_SetsMoveIntent_TowardTarget()
    {
        var grid = new Grid(9, 7);
        var enemy = CreateEntityAt(1, "Goblin", 0, 0);
        var player = CreateEntityAt(2, "Player", 5, 0);

        EnemyIntentPlanner.Plan(enemy, player, grid);

        var intent = enemy.Get<Intent>()!;
        Assert.Equal(IntentKind.Move, intent.Kind);
        Assert.Equal(new GridPoint(1, 0), intent.TargetPosition);
    }
}
