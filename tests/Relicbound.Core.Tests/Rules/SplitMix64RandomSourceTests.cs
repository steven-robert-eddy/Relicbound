using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Rules;

public class SplitMix64RandomSourceTests
{
    [Fact]
    public void NextInt_WithSameSeed_ProducesSameSequence()
    {
        var a = new SplitMix64RandomSource(12345);
        var b = new SplitMix64RandomSource(12345);

        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(a.NextInt(0, 100), b.NextInt(0, 100));
        }
    }

    [Fact]
    public void NextInt_StaysWithinRequestedRange()
    {
        var random = new SplitMix64RandomSource(1);

        for (var i = 0; i < 1000; i++)
        {
            var value = random.NextInt(5, 10);
            Assert.InRange(value, 5, 9);
        }
    }
}
