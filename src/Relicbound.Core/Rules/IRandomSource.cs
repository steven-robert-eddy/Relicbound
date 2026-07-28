namespace Relicbound.Core.Rules;

public interface IRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);
    ulong State { get; }
}
