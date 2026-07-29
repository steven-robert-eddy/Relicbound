using System.Collections.Generic;
using System.Linq;

namespace Relicbound.Gameplay.Expeditions;

public sealed record ExpeditionMap(
    IReadOnlyList<ExpeditionNode> Nodes,
    ExpeditionNodeId StartNodeId,
    ExpeditionNodeId BossNodeId)
{
    public ExpeditionNode Get(ExpeditionNodeId id) => Nodes.First(n => n.Id == id);
}
