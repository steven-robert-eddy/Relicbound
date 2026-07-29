using System.Collections.Generic;
using System.Linq;

namespace Relicbound.Gameplay.Expeditions;

/// <remarks>
/// Milestone 4 issue 4.3. Walks an ExpeditionMap one node at a time:
///
/// - EnterNode moves the pointer onto a node reachable from the current one.
///   Combat/Elite/Boss nodes stay unresolved on entry -- the caller runs the
///   actual fight (Gameplay.Combat) and reports back. Every other node type
///   has no resolution content yet (Milestone 4 issue 4.2 deferred it), so
///   it clears itself the moment you step on it.
/// - ResolveCombat reports the outcome of the fight the caller ran on the
///   current node. A loss ends the expedition (Died). A win clears the node;
///   winning on the Boss node ends the expedition (Complete).
/// - EnterNode refuses to move past an unresolved combat node -- you can't
///   walk around a fight.
/// </remarks>
public sealed class Expedition
{
    private readonly HashSet<ExpeditionNodeId> _cleared = new();

    public Expedition(ExpeditionMap map)
    {
        Map = map;
        CurrentNodeId = map.StartNodeId;
        _cleared.Add(map.StartNodeId);
        Status = ExpeditionStatus.InProgress;
    }

    public ExpeditionMap Map { get; }
    public ExpeditionNodeId CurrentNodeId { get; private set; }
    public ExpeditionStatus Status { get; private set; }

    public ExpeditionNode CurrentNode => Map.Get(CurrentNodeId);
    public bool CurrentNodeIsCleared => _cleared.Contains(CurrentNodeId);
    public bool CurrentNodeRequiresCombat => RequiresCombat(CurrentNode.Type);

    public bool EnterNode(ExpeditionNodeId nodeId)
    {
        if (Status != ExpeditionStatus.InProgress) { return false; }
        if (!CurrentNodeIsCleared) { return false; }
        if (!CurrentNode.NextNodeIds.Contains(nodeId)) { return false; }

        CurrentNodeId = nodeId;

        if (!CurrentNodeRequiresCombat)
        {
            _cleared.Add(nodeId);
        }

        return true;
    }

    public bool ResolveCombat(bool playerWon)
    {
        if (Status != ExpeditionStatus.InProgress || !CurrentNodeRequiresCombat || CurrentNodeIsCleared)
        {
            return false;
        }

        if (!playerWon)
        {
            Status = ExpeditionStatus.Died;
            return true;
        }

        _cleared.Add(CurrentNodeId);

        if (CurrentNode.Type == NodeType.Boss)
        {
            Status = ExpeditionStatus.Complete;
        }

        return true;
    }

    private static bool RequiresCombat(NodeType type) => type is NodeType.Combat or NodeType.Elite or NodeType.Boss;
}
