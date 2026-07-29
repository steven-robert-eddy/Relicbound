using System.Collections.Generic;
using System.Linq;
using Relicbound.Gameplay.Expeditions;
using Xunit;

namespace Relicbound.Gameplay.Tests.Expeditions;

public class ExpeditionMapGeneratorTests
{
    [Fact]
    public void Generate_WithSameSeed_ProducesAnIdenticalMap()
    {
        var a = ExpeditionMapGenerator.Generate(12345);
        var b = ExpeditionMapGenerator.Generate(12345);

        Assert.Equal(a.StartNodeId, b.StartNodeId);
        Assert.Equal(a.BossNodeId, b.BossNodeId);
        Assert.Equal(a.Nodes.Count, b.Nodes.Count);

        foreach (var (nodeA, nodeB) in a.Nodes.Zip(b.Nodes))
        {
            Assert.Equal(nodeA.Id, nodeB.Id);
            Assert.Equal(nodeA.Type, nodeB.Type);
            Assert.Equal(nodeA.Floor, nodeB.Floor);
            Assert.Equal(nodeA.NextNodeIds, nodeB.NextNodeIds);
        }
    }

    [Fact]
    public void Generate_WithDifferentSeeds_ProducesDifferentMaps()
    {
        var a = ExpeditionMapGenerator.Generate(1);
        var b = ExpeditionMapGenerator.Generate(2);

        var typesA = a.Nodes.Select(n => n.Type).ToList();
        var typesB = b.Nodes.Select(n => n.Type).ToList();

        Assert.NotEqual(typesA, typesB);
    }

    [Fact]
    public void Generate_HasExactlyOneStartNodeAndOneBossNode()
    {
        var map = ExpeditionMapGenerator.Generate(7);

        Assert.Single(map.Nodes, n => n.Type == NodeType.Start);
        Assert.Single(map.Nodes, n => n.Type == NodeType.Boss);
        Assert.Equal(NodeType.Start, map.Get(map.StartNodeId).Type);
        Assert.Equal(NodeType.Boss, map.Get(map.BossNodeId).Type);
    }

    [Fact]
    public void Generate_MatchesTheConfiguredFloorCountAndNodesPerFloorRange()
    {
        var config = new ExpeditionMapConfig(FloorCount: 4, MinNodesPerFloor: 2, MaxNodesPerFloor: 2);
        var map = ExpeditionMapGenerator.Generate(99, config);

        // Start (floor 0) + 4 choice floors x 2 nodes + Boss (floor 5).
        Assert.Equal(1 + 4 * 2 + 1, map.Nodes.Count);

        for (var floor = 1; floor <= config.FloorCount; floor++)
        {
            Assert.Equal(config.MinNodesPerFloor, map.Nodes.Count(n => n.Floor == floor));
        }

        Assert.Equal(config.FloorCount + 1, map.Get(map.BossNodeId).Floor);
    }

    [Fact]
    public void Generate_EveryNonBossNode_HasAtLeastOneOutgoingEdge()
    {
        var map = ExpeditionMapGenerator.Generate(42);

        foreach (var node in map.Nodes.Where(n => n.Type != NodeType.Boss))
        {
            Assert.NotEmpty(node.NextNodeIds);
        }
    }

    [Fact]
    public void Generate_EveryNonStartNode_HasAtLeastOneIncomingEdge()
    {
        var map = ExpeditionMapGenerator.Generate(42);

        var withIncoming = new HashSet<ExpeditionNodeId>(map.Nodes.SelectMany(n => n.NextNodeIds));

        foreach (var node in map.Nodes.Where(n => n.Type != NodeType.Start))
        {
            Assert.Contains(node.Id, withIncoming);
        }
    }

    [Fact]
    public void Generate_EveryEdge_PointsExactlyOneFloorAhead()
    {
        var map = ExpeditionMapGenerator.Generate(42);

        foreach (var node in map.Nodes)
        {
            foreach (var nextId in node.NextNodeIds)
            {
                Assert.Equal(node.Floor + 1, map.Get(nextId).Floor);
            }
        }
    }

    [Fact]
    public void Generate_EveryNode_IsReachableFromStart()
    {
        var map = ExpeditionMapGenerator.Generate(42);

        var visited = new HashSet<ExpeditionNodeId> { map.StartNodeId };
        var frontier = new Queue<ExpeditionNodeId>();
        frontier.Enqueue(map.StartNodeId);

        while (frontier.Count > 0)
        {
            var current = map.Get(frontier.Dequeue());
            foreach (var nextId in current.NextNodeIds)
            {
                if (visited.Add(nextId))
                {
                    frontier.Enqueue(nextId);
                }
            }
        }

        Assert.Equal(map.Nodes.Count, visited.Count);
    }
}
