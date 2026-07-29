using Relicbound.Content;
using Relicbound.Content.Encounters;
using Relicbound.Gameplay.Encounters;
using Xunit;

namespace Relicbound.Gameplay.Tests.Encounters;

public class EncounterContentLoaderTests
{
    private const string GoblinPatrolJson = """
        {
          "id": "goblin_patrol",
          "name": "Goblin Patrol",
          "difficultyTier": "STANDARD",
          "enemies": [
            { "id": "goblin", "name": "Goblin", "health": 20 }
          ]
        }
        """;

    [Fact]
    public void LoadAll_WithValidEncounter_ParsesEveryField()
    {
        var definitions = EncounterContentLoader.LoadAll(new[] { GoblinPatrolJson });

        var patrol = Assert.Single(definitions);
        Assert.Equal("goblin_patrol", patrol.Id);
        Assert.Equal("Goblin Patrol", patrol.Name);
        Assert.Equal(DifficultyTier.Standard, patrol.DifficultyTier);
        var enemy = Assert.Single(patrol.Enemies);
        Assert.Equal("goblin", enemy.Id);
        Assert.Equal("Goblin", enemy.Name);
        Assert.Equal(20, enemy.Health);
    }

    [Fact]
    public void LoadAll_WithMultipleEnemies_ParsesEachOne()
    {
        const string json = """
            {
              "id": "goblin_warband",
              "name": "Goblin Warband",
              "difficultyTier": "STANDARD",
              "enemies": [
                { "id": "goblin_1", "name": "Goblin", "health": 15 },
                { "id": "goblin_2", "name": "Goblin", "health": 15 }
              ]
            }
            """;

        var definitions = EncounterContentLoader.LoadAll(new[] { json });

        var warband = Assert.Single(definitions);
        Assert.Equal(2, warband.Enemies.Count);
    }

    [Fact]
    public void LoadAll_WithInvalidDifficultyTier_ReportsError()
    {
        const string json = """
            {
              "id": "bad_encounter",
              "name": "Bad Encounter",
              "difficultyTier": "NIGHTMARE",
              "enemies": [
                { "id": "goblin", "name": "Goblin", "health": 20 }
              ]
            }
            """;

        var exception = Assert.Throws<EncounterContentLoadException>(() => EncounterContentLoader.LoadAll(new[] { json }));

        Assert.Contains(exception.Errors, e => e.Contains("difficultyTier"));
    }

    [Fact]
    public void LoadAll_WithNoEnemies_ReportsError()
    {
        const string json = """
            {
              "id": "empty_encounter",
              "name": "Empty Encounter",
              "difficultyTier": "STANDARD",
              "enemies": []
            }
            """;

        var exception = Assert.Throws<EncounterContentLoadException>(() => EncounterContentLoader.LoadAll(new[] { json }));

        Assert.Contains(exception.Errors, e => e.Contains("enemies"));
    }

    [Fact]
    public void LoadAll_WithDuplicateIds_ReportsIt()
    {
        var exception = Assert.Throws<EncounterContentLoadException>(
            () => EncounterContentLoader.LoadAll(new[] { GoblinPatrolJson, GoblinPatrolJson }));

        Assert.Contains(exception.Errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void LoadEmbedded_LoadsAllFourEncounters_FromRealContentFiles()
    {
        var definitions = EncounterContentLoader.LoadEmbedded(ContentAssembly.Reference);

        Assert.Contains(definitions, d => d.Id == "goblin_patrol" && d.DifficultyTier == DifficultyTier.Standard);
        Assert.Contains(definitions, d => d.Id == "goblin_warband" && d.DifficultyTier == DifficultyTier.Standard);
        Assert.Contains(definitions, d => d.Id == "goblin_chieftain" && d.DifficultyTier == DifficultyTier.Elite);
        Assert.Contains(definitions, d => d.Id == "camp_warlord" && d.DifficultyTier == DifficultyTier.Boss);
    }
}
