using System;
using Relicbound.Core.Entities;

namespace Relicbound.Core.Rules;

public sealed class ActionPointModel : ITurnResourceModel
{
    private const int DefaultActionPoints = 3;

    public int StartingResources(Entity entity) => DefaultActionPoints;

    public bool CanAfford(Entity entity, ActionCost cost)
    {
        return GetResources(entity).ActionPoints >= cost.ActionPoints;
    }

    public void Spend(Entity entity, ActionCost cost)
    {
        var resources = GetResources(entity);
        if (resources.ActionPoints < cost.ActionPoints)
        {
            throw new InvalidOperationException(
                $"{entity.Name} cannot afford a cost of {cost.ActionPoints} AP with only {resources.ActionPoints} remaining.");
        }

        resources.ActionPoints -= cost.ActionPoints;
    }

    public void Refund(Entity entity, int amount)
    {
        GetResources(entity).ActionPoints += amount;
    }

    public void Replenish(Entity entity)
    {
        GetResources(entity).ActionPoints = StartingResources(entity);
    }

    private static TurnResources GetResources(Entity entity)
    {
        var resources = entity.Get<TurnResources>();
        if (resources is null)
        {
            throw new InvalidOperationException($"{entity.Name} has no TurnResources component.");
        }

        return resources;
    }
}
