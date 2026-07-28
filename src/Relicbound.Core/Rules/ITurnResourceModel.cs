using Relicbound.Core.Entities;

namespace Relicbound.Core.Rules;

public interface ITurnResourceModel
{
    int StartingResources(Entity entity);
    bool CanAfford(Entity entity, ActionCost cost);
    void Spend(Entity entity, ActionCost cost);
    void Refund(Entity entity, int amount);
    void Replenish(Entity entity);
}
