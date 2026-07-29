using Relicbound.Content;
using Relicbound.Content.Spells;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Spells;
using Xunit;

namespace Relicbound.Gameplay.Tests.Spells;

public class SpellContentLoaderTests
{
    private const string BoltJson = """
        {
          "id": "bolt",
          "name": "Bolt",
          "tags": ["Void"],
          "cost": 1,
          "range": 4,
          "sockets": 2,
          "targeting": "SINGLE",
          "effects": [
            { "type": "DAMAGE", "value": 6 }
          ]
        }
        """;

    [Fact]
    public void LoadAll_WithValidSpell_ParsesEveryField()
    {
        var definitions = SpellContentLoader.LoadAll(new[] { BoltJson });

        var bolt = Assert.Single(definitions);
        Assert.Equal("bolt", bolt.Id);
        Assert.Equal("Bolt", bolt.Name);
        Assert.Equal(new[] { Tag.Void }, bolt.Tags);
        Assert.Equal(1, bolt.Cost);
        Assert.Equal(4, bolt.Range);
        Assert.Equal(2, bolt.Sockets);
        Assert.Equal(TargetingMode.Single, bolt.Targeting);
        var effect = Assert.Single(bolt.Effects);
        Assert.Equal(6, effect.Value);
    }

    [Fact]
    public void LoadAll_MissingEffects_ReportsError()
    {
        const string json = """
            {
              "id": "no_effects",
              "name": "No Effects",
              "tags": [],
              "cost": 1,
              "range": 1,
              "sockets": 0
            }
            """;

        var exception = Assert.Throws<SpellContentLoadException>(() => SpellContentLoader.LoadAll(new[] { json }));

        Assert.Contains(exception.Errors, e => e.Contains("effects"));
    }

    [Fact]
    public void LoadAll_WithDuplicateIds_ReportsIt()
    {
        var exception = Assert.Throws<SpellContentLoadException>(
            () => SpellContentLoader.LoadAll(new[] { BoltJson, BoltJson }));

        Assert.Contains(exception.Errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void LoadEmbedded_LoadsBoltStrikeAndShield_FromRealContentFiles()
    {
        var definitions = SpellContentLoader.LoadEmbedded(ContentAssembly.Reference);

        Assert.Contains(definitions, d => d.Id == "bolt");
        Assert.Contains(definitions, d => d.Id == "strike");
        Assert.Contains(definitions, d => d.Id == "shield");
    }
}
