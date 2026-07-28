using Relicbound.Core.Effects;
using Relicbound.Core.Tags;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class EffectTagsTests
{
    [Fact]
    public void DamageEffect_WithNoTagsGiven_HasEmptyTags()
    {
        var effect = new DamageEffect(5);

        Assert.Empty(effect.Tags);
    }

    [Fact]
    public void DamageEffect_WithTagsGiven_CarriesThem()
    {
        var effect = new DamageEffect(5, new[] { Tag.Fire });

        Assert.Contains(Tag.Fire, effect.Tags);
    }
}
