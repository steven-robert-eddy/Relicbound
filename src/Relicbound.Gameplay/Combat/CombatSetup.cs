using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Gameplay.Spells;

namespace Relicbound.Gameplay.Combat;

/// <remarks>
/// BasicAttack is the composed spell every entity's plain attack action
/// resolves -- see CombatSimulation.RequestAttack. It is passed in already
/// composed (Strike with no runes socketed, or whatever a caller like the
/// Spell Forge panel composed instead) rather than loaded here, since
/// Gameplay cannot depend on Relicbound.Content; the caller (the Godot
/// view, or a test) loads and composes it.
/// </remarks>
public sealed record CombatSetup(
    int GridWidth,
    int GridHeight,
    IReadOnlyList<Entity> Entities,
    ulong RandomSeed,
    ComposedSpell BasicAttack);
