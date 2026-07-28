using System.Collections.Generic;
using Relicbound.Core.Tags;

namespace Relicbound.Core.Entities;

public sealed class EntityTags : IComponent
{
    public EntityTags(IReadOnlyCollection<Tag> values)
    {
        Values = values;
    }

    public IReadOnlyCollection<Tag> Values { get; }
}
