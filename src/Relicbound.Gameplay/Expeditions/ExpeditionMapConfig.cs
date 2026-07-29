namespace Relicbound.Gameplay.Expeditions;

/// <remarks>
/// Defaults land near the "3 floors, ~12 nodes" figure docs/GAME_DESIGN.md
/// section 8 proposes, which that doc explicitly flags as needing
/// playtesting rather than a fixed spec -- keeping these as constructor
/// parameters, not constants, is what lets that number move without
/// touching ExpeditionMapGenerator itself.
/// </remarks>
public sealed record ExpeditionMapConfig(
    int FloorCount = 3,
    int MinNodesPerFloor = 3,
    int MaxNodesPerFloor = 4);
