namespace Relicbound.Core.Rules;

public interface IRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);

    /// <remarks>
    /// A raw 64-bit draw, meant to seed an independent child IRandomSource --
    /// see docs/TECHNICAL_ARCHITECTURE.md section 2: a run seed splits into
    /// distinct map, loot, and combat streams so that one subsystem drawing
    /// an extra roll never shifts another's sequence.
    /// </remarks>
    ulong NextSeed();

    ulong State { get; }
}
