using System.Linq;
using Relicbound.Gameplay.Expeditions;
using Xunit;

namespace Relicbound.Gameplay.Tests.Expeditions;

public class ExpeditionTests
{
    // FloorCount: 1 keeps this small and legible -- Start -> one choice
    // floor -> Boss. No encounters supplied: node types are what matter
    // here, not which encounter a Combat/Elite node drew.
    private static ExpeditionMap SmallMap(ulong seed = 1) =>
        ExpeditionMapGenerator.Generate(seed, new ExpeditionMapConfig(FloorCount: 1, MinNodesPerFloor: 3, MaxNodesPerFloor: 3));

    [Fact]
    public void NewExpedition_StartsOnTheStartNode_AlreadyCleared_InProgress()
    {
        var expedition = new Expedition(SmallMap());

        Assert.Equal(ExpeditionStatus.InProgress, expedition.Status);
        Assert.Equal(expedition.Map.StartNodeId, expedition.CurrentNodeId);
        Assert.True(expedition.CurrentNodeIsCleared);
    }

    [Fact]
    public void EnterNode_ToANodeNotConnectedToTheCurrentOne_Fails()
    {
        var expedition = new Expedition(SmallMap());
        var bossId = expedition.Map.BossNodeId;

        var entered = expedition.EnterNode(bossId);

        Assert.False(entered);
        Assert.Equal(expedition.Map.StartNodeId, expedition.CurrentNodeId);
    }

    [Fact]
    public void EnterNode_ToANonCombatNode_ClearsItImmediately()
    {
        var map = SmallMap();
        var noFightNode = map.Nodes.First(n => n.Type is NodeType.Treasure or NodeType.Event or NodeType.Merchant);
        var expedition = new Expedition(map);

        var entered = expedition.EnterNode(noFightNode.Id);

        Assert.True(entered);
        Assert.Equal(noFightNode.Id, expedition.CurrentNodeId);
        Assert.True(expedition.CurrentNodeIsCleared);
        Assert.Equal(ExpeditionStatus.InProgress, expedition.Status);
    }

    [Fact]
    public void EnterNode_ToACombatNode_LeavesItUnresolved()
    {
        var map = SmallMap();
        var combatNode = map.Nodes.First(n => n.Type is NodeType.Combat or NodeType.Elite);
        var expedition = new Expedition(map);

        expedition.EnterNode(combatNode.Id);

        Assert.True(expedition.CurrentNodeRequiresCombat);
        Assert.False(expedition.CurrentNodeIsCleared);
    }

    [Fact]
    public void EnterNode_PastAnUnresolvedCombatNode_Fails()
    {
        var map = SmallMap();
        var combatNode = map.Nodes.First(n => n.Type is NodeType.Combat or NodeType.Elite);
        var expedition = new Expedition(map);
        expedition.EnterNode(combatNode.Id);

        var nextChoice = combatNode.NextNodeIds[0];
        var entered = expedition.EnterNode(nextChoice);

        Assert.False(entered);
        Assert.Equal(combatNode.Id, expedition.CurrentNodeId);
    }

    [Fact]
    public void ResolveCombat_ALoss_EndsTheExpeditionAsDied()
    {
        var map = SmallMap();
        var combatNode = map.Nodes.First(n => n.Type is NodeType.Combat or NodeType.Elite);
        var expedition = new Expedition(map);
        expedition.EnterNode(combatNode.Id);

        var resolved = expedition.ResolveCombat(playerWon: false);

        Assert.True(resolved);
        Assert.Equal(ExpeditionStatus.Died, expedition.Status);
    }

    [Fact]
    public void ResolveCombat_AWinOnANonBossNode_ClearsIt_ButStaysInProgress()
    {
        var map = SmallMap();
        var combatNode = map.Nodes.First(n => n.Type is NodeType.Combat or NodeType.Elite);
        var expedition = new Expedition(map);
        expedition.EnterNode(combatNode.Id);

        var resolved = expedition.ResolveCombat(playerWon: true);

        Assert.True(resolved);
        Assert.True(expedition.CurrentNodeIsCleared);
        Assert.Equal(ExpeditionStatus.InProgress, expedition.Status);
    }

    [Fact]
    public void ResolveCombat_AWinOnTheBossNode_CompletesTheExpedition()
    {
        var map = SmallMap();
        var expedition = new Expedition(map);

        // Walk to the Boss the same way a player would: through one of the
        // choice-floor nodes, resolving combat along the way as needed.
        var choiceNode = map.Get(map.Get(map.StartNodeId).NextNodeIds[0]);
        expedition.EnterNode(choiceNode.Id);
        if (expedition.CurrentNodeRequiresCombat)
        {
            expedition.ResolveCombat(playerWon: true);
        }

        expedition.EnterNode(map.BossNodeId);
        var resolved = expedition.ResolveCombat(playerWon: true);

        Assert.True(resolved);
        Assert.Equal(ExpeditionStatus.Complete, expedition.Status);
    }

    [Fact]
    public void ResolveCombat_WhenTheCurrentNodeDoesNotRequireCombat_IsANoOp()
    {
        var map = SmallMap();
        var noFightNode = map.Nodes.First(n => n.Type is NodeType.Treasure or NodeType.Event or NodeType.Merchant);
        var expedition = new Expedition(map);
        expedition.EnterNode(noFightNode.Id);

        var resolved = expedition.ResolveCombat(playerWon: true);

        Assert.False(resolved);
        Assert.Equal(ExpeditionStatus.InProgress, expedition.Status);
    }

    [Fact]
    public void EnterNode_AfterTheExpeditionHasEnded_Fails()
    {
        var map = SmallMap();
        var combatNode = map.Nodes.First(n => n.Type is NodeType.Combat or NodeType.Elite);
        var expedition = new Expedition(map);
        expedition.EnterNode(combatNode.Id);
        expedition.ResolveCombat(playerWon: false);

        var entered = expedition.EnterNode(combatNode.NextNodeIds[0]);

        Assert.False(entered);
        Assert.Equal(ExpeditionStatus.Died, expedition.Status);
    }
}
