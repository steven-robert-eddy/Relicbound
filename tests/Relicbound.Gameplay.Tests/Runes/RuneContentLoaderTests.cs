using Relicbound.Content;
using Relicbound.Content.Runes;
using Relicbound.Core.Tags;
using Xunit;

namespace Relicbound.Gameplay.Tests.Runes;

public class RuneContentLoaderTests
{
    private const string FlameRuneJson = """
        {
          "id": "rune_flame",
          "name": "Flame Rune",
          "namePrefix": "Inferno",
          "addsTags": ["Fire"],
          "costDelta": 0,
          "effects": [
            { "type": "APPLY_STATUS", "status": "BURN", "stacks": 2, "durationRounds": 3 }
          ]
        }
        """;

    [Fact]
    public void LoadAll_WithValidRune_ParsesEveryField()
    {
        var definitions = RuneContentLoader.LoadAll(new[] { FlameRuneJson });

        var flame = Assert.Single(definitions);
        Assert.Equal("rune_flame", flame.Id);
        Assert.Equal("Flame Rune", flame.Name);
        Assert.Equal("Inferno", flame.NamePrefix);
        Assert.Equal(new[] { Tag.Fire }, flame.AddsTags);
        Assert.Equal(0, flame.CostDelta);
        Assert.Single(flame.AddsEffects);
        Assert.Null(flame.ChainAdditionalTargets);
    }

    [Fact]
    public void LoadAll_ChainRune_ParsesChainFields()
    {
        const string json = """
            {
              "id": "rune_chain",
              "name": "Chain Rune",
              "namePrefix": "Chain",
              "addsTags": [],
              "costDelta": 1,
              "effects": [],
              "chainAdditionalTargets": 2,
              "chainFalloffPercent": 50
            }
            """;

        var definitions = RuneContentLoader.LoadAll(new[] { json });

        var chain = Assert.Single(definitions);
        Assert.Equal(2, chain.ChainAdditionalTargets);
        Assert.Equal(50, chain.ChainFalloffPercent);
    }

    [Fact]
    public void LoadAll_ChainAdditionalTargetsWithoutFalloff_ReportsError()
    {
        const string json = """
            {
              "id": "rune_bad_chain",
              "name": "Bad Chain Rune",
              "namePrefix": "Bad",
              "addsTags": [],
              "costDelta": 1,
              "effects": [],
              "chainAdditionalTargets": 2
            }
            """;

        var exception = Assert.Throws<RuneContentLoadException>(() => RuneContentLoader.LoadAll(new[] { json }));

        Assert.Contains(exception.Errors, e => e.Contains("chainFalloffPercent"));
    }

    [Fact]
    public void LoadAll_WithDuplicateIds_ReportsIt()
    {
        var exception = Assert.Throws<RuneContentLoadException>(
            () => RuneContentLoader.LoadAll(new[] { FlameRuneJson, FlameRuneJson }));

        Assert.Contains(exception.Errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void LoadEmbedded_LoadsFlameAndChainRunes_FromRealContentFiles()
    {
        var definitions = RuneContentLoader.LoadEmbedded(ContentAssembly.Reference);

        Assert.Contains(definitions, d => d.Id == "rune_flame");
        Assert.Contains(definitions, d => d.Id == "rune_chain");
    }
}
