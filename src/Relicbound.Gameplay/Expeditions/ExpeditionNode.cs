using System.Collections.Generic;

namespace Relicbound.Gameplay.Expeditions;

/// <remarks>
/// Floor 0 is always the single Start node; the highest floor is always the
/// single Boss node. NextNodeIds only ever points at nodes one floor ahead --
/// the map is a DAG by construction, not something callers need to verify.
///
/// EncounterId and GuaranteesArtifactReward are Milestone 4 issue 4.2's
/// minimal per-type resolution data -- docs/GAME_DESIGN.md section 8:
///
/// - Combat, Elite, Boss: EncounterId is a seeded pick from the matching
///   Relicbound.Gameplay.Encounters.DifficultyTier pool, or null if the
///   caller didn't supply one for that tier (ExpeditionMapGenerator.Generate
///   works fine with no encounters at all -- useful for shape-only tests).
/// - Elite: GuaranteesArtifactReward is always true ("harder fight,
///   guaranteed artifact").
/// - Treasure, Event, Merchant: no resolution data yet. What each of those
///   actually does is later milestone work (4.4 rewards in particular);
///   the node type alone is the marker for now.
/// </remarks>
public sealed record ExpeditionNode(
    ExpeditionNodeId Id,
    NodeType Type,
    int Floor,
    IReadOnlyList<ExpeditionNodeId> NextNodeIds,
    string? EncounterId = null,
    bool GuaranteesArtifactReward = false);
