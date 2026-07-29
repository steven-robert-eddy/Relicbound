using System;
using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Rules;

namespace Relicbound.Gameplay.Expeditions;

/// <remarks>
/// Milestone 4 issue 4.1. Builds the branching-then-converging shape from
/// docs/GAME_DESIGN.md section 8: one Start node, a run of floors each
/// holding a handful of node-type choices, and one Boss node every path on
/// the final floor leads into.
///
/// Deterministic: the same seed always produces the same map (structure and
/// node types alike), per docs/TECHNICAL_ARCHITECTURE.md section 2 --
/// callers pass the map stream's own seed, already split off the run seed
/// via IRandomSource.NextSeed(), not the run seed itself.
/// </remarks>
public static class ExpeditionMapGenerator
{
    // docs/GAME_DESIGN.md section 8: "Combat -- the default." Weighted so
    // roughly half of any floor's nodes are Combat, with the other four
    // choice types splitting the rest evenly.
    private static readonly NodeType[] WeightedChoicePool =
    {
        NodeType.Combat, NodeType.Combat, NodeType.Combat, NodeType.Combat,
        NodeType.Elite,
        NodeType.Treasure,
        NodeType.Event,
        NodeType.Merchant,
    };

    public static ExpeditionMap Generate(ulong seed, ExpeditionMapConfig? config = null)
    {
        config ??= new ExpeditionMapConfig();

        if (config.FloorCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(config), "FloorCount must be at least 1.");
        }

        if (config.MinNodesPerFloor < 1 || config.MaxNodesPerFloor < config.MinNodesPerFloor)
        {
            throw new ArgumentOutOfRangeException(nameof(config), "MaxNodesPerFloor must be >= MinNodesPerFloor >= 1.");
        }

        var random = new SplitMix64RandomSource(seed);
        var nextId = 0;
        var floorTypes = new Dictionary<ExpeditionNodeId, NodeType>();
        var floorOf = new Dictionary<ExpeditionNodeId, int>();
        var edges = new Dictionary<ExpeditionNodeId, List<ExpeditionNodeId>>();

        ExpeditionNodeId NewNode(NodeType type, int floor)
        {
            var id = new ExpeditionNodeId(nextId++);
            floorTypes[id] = type;
            floorOf[id] = floor;
            edges[id] = new List<ExpeditionNodeId>();
            return id;
        }

        var startId = NewNode(NodeType.Start, floor: 0);
        var previousFloor = new List<ExpeditionNodeId> { startId };

        for (var floor = 1; floor <= config.FloorCount; floor++)
        {
            var nodeCount = random.NextInt(config.MinNodesPerFloor, config.MaxNodesPerFloor + 1);
            var currentFloor = new List<ExpeditionNodeId>(nodeCount);

            for (var i = 0; i < nodeCount; i++)
            {
                var type = WeightedChoicePool[random.NextInt(0, WeightedChoicePool.Length)];
                currentFloor.Add(NewNode(type, floor));
            }

            ConnectFloors(previousFloor, currentFloor, edges, random);
            previousFloor = currentFloor;
        }

        var bossId = NewNode(NodeType.Boss, floor: config.FloorCount + 1);
        ConnectFloors(previousFloor, new List<ExpeditionNodeId> { bossId }, edges, random);

        var nodes = Enumerable.Range(0, nextId)
            .Select(i => new ExpeditionNodeId(i))
            .Select(id => new ExpeditionNode(id, floorTypes[id], floorOf[id], edges[id]))
            .ToList();

        return new ExpeditionMap(nodes, startId, bossId);
    }

    /// <remarks>
    /// Every node in <paramref name="from"/> links to 1-2 nodes in
    /// <paramref name="to"/> (or exactly 1 when there's only one target, e.g.
    /// the Boss floor). A second pass then guarantees every node in
    /// <paramref name="to"/> has at least one incoming link, so a path from
    /// Start always reaches every node the player can see -- no node is ever
    /// unreachable, and no path is ever a dead end short of the Boss.
    /// </remarks>
    private static void ConnectFloors(
        IReadOnlyList<ExpeditionNodeId> from,
        IReadOnlyList<ExpeditionNodeId> to,
        Dictionary<ExpeditionNodeId, List<ExpeditionNodeId>> edges,
        IRandomSource random)
    {
        foreach (var source in from)
        {
            var linkCount = Math.Min(to.Count, random.NextInt(1, 3));
            var chosen = edges[source];

            while (chosen.Count < linkCount)
            {
                var candidate = to[random.NextInt(0, to.Count)];
                if (!chosen.Contains(candidate))
                {
                    chosen.Add(candidate);
                }
            }
        }

        foreach (var target in to)
        {
            var hasIncoming = from.Any(source => edges[source].Contains(target));
            if (!hasIncoming)
            {
                var source = from[random.NextInt(0, from.Count)];
                edges[source].Add(target);
            }
        }
    }
}
