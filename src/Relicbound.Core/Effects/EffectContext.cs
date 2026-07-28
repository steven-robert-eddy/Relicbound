using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Rules;

namespace Relicbound.Core.Effects;

public sealed class EffectContext
{
    public EffectContext(
        Entity source,
        IReadOnlyList<Entity> targets,
        IRandomSource random,
        int depth,
        EffectOrigin origin)
    {
        Source = source;
        Targets = targets;
        Random = random;
        Depth = depth;
        Origin = origin;
    }

    public Entity Source { get; }
    public IReadOnlyList<Entity> Targets { get; }
    public IRandomSource Random { get; }
    public int Depth { get; }
    public EffectOrigin Origin { get; }
}
