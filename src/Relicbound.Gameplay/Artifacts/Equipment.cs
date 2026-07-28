using System;
using System.Collections.Generic;
using Relicbound.Core.Entities;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Three artifact slots, per docs/GAME_DESIGN.md section 6. Pure data --
/// mutating a slot has no side effects of its own. Publishing
/// ArtifactEquipped/Unequipped and wiring TriggerRegistry is
/// ArtifactEquipper's job, not this component's, so this stays a plain
/// component like Health or StatBlock.
///
/// This lives in Gameplay rather than Core because it holds
/// ArtifactDefinition, a Gameplay-level type -- Core cannot reference it.
/// </remarks>
public sealed class Equipment : IComponent
{
    public const int SlotCount = 3;

    private readonly ArtifactDefinition?[] _slots = new ArtifactDefinition?[SlotCount];

    public IReadOnlyList<ArtifactDefinition?> Slots => _slots;

    public ArtifactDefinition? Get(int slot)
    {
        ValidateSlot(slot);
        return _slots[slot];
    }

    /// <returns>whatever was previously in the slot, if anything.</returns>
    public ArtifactDefinition? Equip(int slot, ArtifactDefinition artifact)
    {
        ValidateSlot(slot);
        var previous = _slots[slot];
        _slots[slot] = artifact;
        return previous;
    }

    /// <returns>whatever was in the slot, if anything.</returns>
    public ArtifactDefinition? Unequip(int slot)
    {
        ValidateSlot(slot);
        var previous = _slots[slot];
        _slots[slot] = null;
        return previous;
    }

    private static void ValidateSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), $"Slot must be between 0 and {SlotCount - 1}.");
        }
    }
}
