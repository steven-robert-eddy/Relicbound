using System.Collections.Generic;
using System.Linq;

namespace Relicbound.Core.Tags;

public static class TagMatching
{
    public static bool Overlaps(IReadOnlyCollection<Tag> a, IReadOnlyCollection<Tag> b)
    {
        return a.Any(b.Contains);
    }
}
