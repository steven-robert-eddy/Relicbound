using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Combat;
using Relicbound.Gameplay.Spells;
using Xunit;

namespace Relicbound.Gameplay.Tests.Combat;

public class CombatSimulationArtifactTests
{
    // Same numbers the old hardcoded StrikeDamage/AttackCost constants used --
    // see CombatSimulationTests.BasicAttackSpell for the same convention.
    private static ComposedSpell BasicAttackSpell() => new(
        Name: "Strike",
        Cost: 1,
        Tags: System.Array.Empty<Tag>(),
        Targeting: TargetingMode.Single,
        ChainAdditionalTargets: null,
        ChainFalloffPercent: null,
        Effects: new Effect[] { new DamageEffect(5) });

    private static ArtifactDefinition EmberHeart() => new(
        Id: "ember_heart",
        Name: "Ember Heart",
        Tags: new[] { Tag.Fire, Tag.Spirit },
        Trigger: EventType.DamageDealt,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.ApplyStatus, Status: StatusType.Burn, Stacks: 2, DurationRounds: 3) });

    private static ArtifactDefinition PhoenixFeather() => new(
        Id: "phoenix_feather",
        Name: "Phoenix Feather",
        Tags: new[] { Tag.Fire, Tag.Spirit },
        Trigger: EventType.PlayerDefeated,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Revive, Value: 50) },
        HolderRole: TriggerHolderRole.Recipient,
        EffectTarget: EffectTargetSelector.Self);

    private static (CombatSimulation Sim, Entity Player, Entity Goblin) CreateSimulation(
        int playerHealth = 30, int enemyHealth = 20, int enemyX = 8, int enemyY = 3, Equipment? playerEquipment = null)
    {
        var player = new Entity(new EntityId(1), "Player");
        player.Add(new PlayerControlled());
        player.Add(new Health(playerHealth));
        player.Add(new GridPosition(new GridPoint(3, 3)));
        if (playerEquipment is not null)
        {
            player.Add(playerEquipment);
        }

        var goblin = new Entity(new EntityId(2), "Goblin");
        goblin.Add(new Health(enemyHealth));
        goblin.Add(new GridPosition(new GridPoint(enemyX, enemyY)));

        var setup = new CombatSetup(9, 7, new Entity[] { player, goblin }, RandomSeed: 1, BasicAttackSpell());
        return (new CombatSimulation(setup), player, goblin);
    }

    [Fact]
    public void EquippedEmberHeart_AppliesBurn_WhenItsHolderDealsDamage()
    {
        // Equip before the simulation is constructed, so it's the
        // constructor's own pre-equipped registration that gets exercised --
        // not a manual TriggerRegistry.Register call from the test.
        var equipment = new Equipment();
        equipment.Equip(0, EmberHeart());

        // Adjacent, so the player can attack this turn.
        var (sim, player, goblin) = CreateSimulation(enemyX: 4, enemyY: 3, playerEquipment: equipment);

        sim.RequestAttack(player.Id, goblin.Id);

        Assert.Contains(sim.Journal.Entries, e => e.Type == EventType.ArtifactTriggered);
        Assert.True(goblin.Get<Statuses>()!.Has(StatusType.Burn));
    }

    [Fact]
    public void BurnStatus_DealsDamage_OnRoundStart_ProportionalToStacks()
    {
        var (sim, _, goblin) = CreateSimulation(enemyHealth: 20);
        goblin.Add(new Statuses());
        goblin.Get<Statuses>()!.Add(StatusType.Burn, stacks: 2, durationRounds: 3);
        var healthBeforeTick = goblin.Get<Health>()!.Current;

        sim.EndPlayerTurn();

        // StatusTickEffects: Burn deals 2 damage per stack.
        Assert.Equal(healthBeforeTick - 4, goblin.Get<Health>()!.Current);
    }

    [Fact]
    public void EquippedPhoenixFeather_RevivesThePlayer_WhenTheyAreDefeated()
    {
        var equipment = new Equipment();
        equipment.Equip(0, PhoenixFeather());
        var (sim, player, _) = CreateSimulation(playerHealth: 5, enemyX: 4, enemyY: 3, playerEquipment: equipment);

        // The goblin starts adjacent (Attack intent), so ending the turn
        // without the player acting resolves an attack that defeats them.
        sim.EndPlayerTurn();

        Assert.False(player.Get<Health>()!.IsDefeated);
        Assert.False(sim.IsCombatOver);
        Assert.Contains(sim.Journal.Entries, e => e.Type == EventType.EntityRevived);
    }
}
