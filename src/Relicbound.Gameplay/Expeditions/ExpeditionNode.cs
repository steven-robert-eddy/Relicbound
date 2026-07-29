using System.Collections.Generic;

namespace Relicbound.Gameplay.Expeditions;

/// <remarks>
/// Floor 0 is always the single Start node; the highest floor is always the
/// single Boss node. NextNodeIds only ever points at nodes one floor ahead --
/// the map is a DAG by construction, not something callers need to verify.
/// </remarks>
public sealed record ExpeditionNode(
    ExpeditionNodeId Id,
    NodeType Type,
    int Floor,
    IReadOnlyList<ExpeditionNodeId> NextNodeIds);
