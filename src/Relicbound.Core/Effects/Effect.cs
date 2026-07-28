using System;
using System.Collections.Generic;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Effects;

public abstract class Effect
{
    protected Effect(IReadOnlyCollection<Tag>? tags = null)
    {
        Tags = tags ?? Array.Empty<Tag>();
    }

    /// <remarks>
    /// Lets a spell/artifact-authored effect carry an elemental or mechanical
    /// identity (e.g. a Fire-tagged DamageEffect) independent of what status
    /// or stat it happens to touch. See docs/GAME_DESIGN.md section 5.
    /// </remarks>
    public IReadOnlyCollection<Tag> Tags { get; }

    public abstract EffectResult Execute(EffectContext context);
}
