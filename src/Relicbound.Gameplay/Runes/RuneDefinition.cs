using System.Collections.Generic;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;

namespace Relicbound.Gameplay.Runes;

/// <remarks>
/// A rune: pure data, socketed into a spell in socket order. ChainAdditionalTargets
/// and ChainFalloffPercent are populated only for a Chain-modifying rune -- see
/// docs/GAME_DESIGN.md section 7. Kept as flat nullable fields (matching
/// EffectDefinition's own shape) rather than a generic modifier list: there
/// is exactly one modifier kind today, and a list earns its keep only once a
/// second one actually exists.
///
/// FalloffPercent is an integer percentage (50 means each subsequent chain
/// target takes 50% of the previous target's damage), not a float, to match
/// the integer fixed-point convention StatBlock already established for the
/// same determinism reasons -- see docs/CODING_STANDARDS.md.
/// </remarks>
public sealed record RuneDefinition(
    string Id,
    string Name,
    string NamePrefix,
    IReadOnlyList<Tag> AddsTags,
    int CostDelta,
    IReadOnlyList<EffectDefinition> AddsEffects,
    int? ChainAdditionalTargets = null,
    int? ChainFalloffPercent = null);
