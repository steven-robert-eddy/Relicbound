using System;

namespace Relicbound.Core.Rules;

/// <remarks>
/// SplitMix64 (Vigna) rather than System.Random: it is fully specified, so
/// the same seed produces the same sequence on any platform or .NET version
/// — the property Relicbound's determinism rule depends on.
/// </remarks>
public sealed class SplitMix64RandomSource : IRandomSource
{
    private ulong _state;

    public SplitMix64RandomSource(ulong seed)
    {
        _state = seed;
    }

    public ulong State => _state;

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
        }

        var range = (ulong)(maxExclusive - minInclusive);
        var value = NextUInt64() % range;
        return minInclusive + (int)value;
    }

    public ulong NextSeed() => NextUInt64();

    private ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        var z = _state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
