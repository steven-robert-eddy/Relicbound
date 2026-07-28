using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Tags;

namespace Relicbound.Gameplay.Combat;

/// <remarks>
/// A bounded, engine-known mapping from status type to what happens on its
/// round-start tick (docs/GAME_DESIGN.md section 4: "Statuses tick (Burn
/// deals damage, Chill decrements, ...)"). This is a switch on StatusType --
/// a small, closed enum the engine owns, the same shape as
/// EffectDefinitionFactory's switch on EffectDefinitionType -- never on an
/// artifact's name or id, so it doesn't run afoul of "content is data".
/// Adding a new status kind means adding a case here; adding an artifact that
/// reuses an existing status never touches this file.
/// </remarks>
public static class StatusTickEffects
{
    private const int BurnDamagePerStack = 2;

    public static Effect? Create(StatusType type, int stacks) => type switch
    {
        StatusType.Burn => new DamageEffect(stacks * BurnDamagePerStack, new[] { Tag.Fire }),
        _ => null,
    };
}
