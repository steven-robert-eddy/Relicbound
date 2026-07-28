using System.Linq;
using Relicbound.Content.Artifacts;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

public class ArtifactContentLoaderTests
{
    private const string EmberHeartJson = """
        {
          "id": "ember_heart",
          "name": "Ember Heart",
          "tags": ["Fire", "Spirit"],
          "trigger": "DAMAGE_DEALT",
          "effects": [
            { "type": "APPLY_STATUS", "status": "BURN", "stacks": 2, "durationRounds": 3 }
          ]
        }
        """;

    [Fact]
    public void LoadAll_WithValidArtifact_ParsesEveryField()
    {
        var definitions = ArtifactContentLoader.LoadAll(new[] { EmberHeartJson });

        var ember = Assert.Single(definitions);
        Assert.Equal("ember_heart", ember.Id);
        Assert.Equal("Ember Heart", ember.Name);
        Assert.Equal(new[] { Tag.Fire, Tag.Spirit }, ember.Tags);
        Assert.Equal(EventType.DamageDealt, ember.Trigger);
        var effect = Assert.Single(ember.Effects);
        Assert.Equal(Relicbound.Core.Entities.StatusType.Burn, effect.Status);
        Assert.Equal(2, effect.Stacks);
        Assert.Equal(3, effect.DurationRounds);
    }

    [Fact]
    public void LoadAll_WithPhoenixFeatherShape_ParsesHolderRoleAndEffectTarget()
    {
        const string json = """
            {
              "id": "phoenix_feather",
              "name": "Phoenix Feather",
              "tags": ["Fire", "Spirit"],
              "trigger": "PLAYER_DEFEATED",
              "holderRole": "Recipient",
              "effectTarget": "Self",
              "effects": [ { "type": "HEAL", "value": 1 } ]
            }
            """;

        var definitions = ArtifactContentLoader.LoadAll(new[] { json });

        var feather = Assert.Single(definitions);
        Assert.Equal(Relicbound.Gameplay.Artifacts.TriggerHolderRole.Recipient, feather.HolderRole);
        Assert.Equal(Relicbound.Gameplay.Artifacts.EffectTargetSelector.Self, feather.EffectTarget);
    }

    [Fact]
    public void LoadAll_WithDuplicateIds_ReportsAllOffendingDocuments()
    {
        var exception = Assert.Throws<ArtifactContentLoadException>(
            () => ArtifactContentLoader.LoadAll(new[] { EmberHeartJson, EmberHeartJson }));

        Assert.Contains(exception.Errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void LoadAll_WithMultipleInvalidArtifacts_ReportsEveryFailure_NotJustTheFirst()
    {
        const string missingId = """
            {
              "name": "No Id",
              "tags": [],
              "trigger": "DAMAGE_DEALT",
              "effects": [ { "type": "HEAL", "value": 1 } ]
            }
            """;

        const string badTrigger = """
            {
              "id": "bad_trigger",
              "name": "Bad Trigger",
              "tags": [],
              "trigger": "NOT_A_REAL_EVENT",
              "effects": [ { "type": "HEAL", "value": 1 } ]
            }
            """;

        const string missingEffectField = """
            {
              "id": "missing_effect_field",
              "name": "Missing Effect Field",
              "tags": [],
              "trigger": "DAMAGE_DEALT",
              "effects": [ { "type": "APPLY_STATUS", "status": "BURN" } ]
            }
            """;

        var exception = Assert.Throws<ArtifactContentLoadException>(
            () => ArtifactContentLoader.LoadAll(new[] { missingId, badTrigger, missingEffectField }));

        Assert.Equal(3, exception.Errors.Count);
    }

    [Fact]
    public void LoadEmbedded_LoadsEmberHeartAndPhoenixFeather_FromRealContentFiles()
    {
        var definitions = ArtifactContentLoader.LoadEmbedded(Relicbound.Content.ContentAssembly.Reference);

        Assert.Contains(definitions, d => d.Id == "ember_heart");
        Assert.Contains(definitions, d => d.Id == "phoenix_feather");
    }

    [Fact]
    public void LoadAll_WithUnknownTag_ReportsIt()
    {
        const string json = """
            {
              "id": "bad_tag",
              "name": "Bad Tag",
              "tags": ["NotATag"],
              "trigger": "DAMAGE_DEALT",
              "effects": [ { "type": "HEAL", "value": 1 } ]
            }
            """;

        var exception = Assert.Throws<ArtifactContentLoadException>(() => ArtifactContentLoader.LoadAll(new[] { json }));

        Assert.Contains(exception.Errors, e => e.Contains("NotATag"));
    }
}
