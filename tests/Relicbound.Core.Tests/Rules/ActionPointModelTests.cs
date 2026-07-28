using Relicbound.Core.Entities;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Rules;

public class ActionPointModelTests
{
    private static Entity CreateEntityWithResources(int startingAp)
    {
        var entity = new Entity(new EntityId(1), "Test");
        entity.Add(new TurnResources(startingAp));
        return entity;
    }

    [Fact]
    public void StartingResources_DefaultsToThree()
    {
        var model = new ActionPointModel();
        var entity = new Entity(new EntityId(1), "Test");

        Assert.Equal(3, model.StartingResources(entity));
    }

    [Fact]
    public void CanAfford_WithEnoughAp_ReturnsTrue()
    {
        var model = new ActionPointModel();
        var entity = CreateEntityWithResources(3);

        Assert.True(model.CanAfford(entity, new ActionCost(2)));
    }

    [Fact]
    public void CanAfford_WithoutEnoughAp_ReturnsFalse()
    {
        var model = new ActionPointModel();
        var entity = CreateEntityWithResources(1);

        Assert.False(model.CanAfford(entity, new ActionCost(2)));
    }

    [Fact]
    public void Spend_ReducesActionPoints()
    {
        var model = new ActionPointModel();
        var entity = CreateEntityWithResources(3);

        model.Spend(entity, new ActionCost(1));

        Assert.Equal(2, entity.Get<TurnResources>()!.ActionPoints);
    }

    [Fact]
    public void Refund_IncreasesActionPoints()
    {
        var model = new ActionPointModel();
        var entity = CreateEntityWithResources(1);

        model.Refund(entity, 2);

        Assert.Equal(3, entity.Get<TurnResources>()!.ActionPoints);
    }

    [Fact]
    public void Replenish_ResetsToStartingResources()
    {
        var model = new ActionPointModel();
        var entity = CreateEntityWithResources(0);

        model.Replenish(entity);

        Assert.Equal(3, entity.Get<TurnResources>()!.ActionPoints);
    }
}
