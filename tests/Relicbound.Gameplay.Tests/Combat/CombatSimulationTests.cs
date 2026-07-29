using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Combat;
using Relicbound.Gameplay.Spells;
using Xunit;

namespace Relicbound.Gameplay.Tests.Combat;

public class CombatSimulationTests
{
    // Same numbers the old hardcoded StrikeDamage/AttackCost constants used,
    // so every existing assertion in this file stays meaningful -- this is
    // deliberately what src/Relicbound.Content/Spells/strike.json describes,
    // just constructed inline rather than loaded, to keep these tests free
    // of file I/O.
    private static ComposedSpell BasicAttackSpell() => new(
        Name: "Strike",
        Cost: 1,
        Tags: System.Array.Empty<Tag>(),
        Targeting: TargetingMode.Single,
        ChainAdditionalTargets: null,
        ChainFalloffPercent: null,
        Effects: new Effect[] { new DamageEffect(5) });

    private static CombatSimulation CreateSimulation(
        int playerHealth = 30, int enemyHealth = 20, int enemyX = 5, int enemyY = 3,
        ComposedSpell? basicAttack = null)
    {
        var player = new Entity(new EntityId(1), "Player");
        player.Add(new PlayerControlled());
        player.Add(new Health(playerHealth));
        player.Add(new GridPosition(new GridPoint(3, 3)));

        var goblin = new Entity(new EntityId(2), "Goblin");
        goblin.Add(new Health(enemyHealth));
        goblin.Add(new GridPosition(new GridPoint(enemyX, enemyY)));

        var setup = new CombatSetup(9, 7, new Entity[] { player, goblin }, RandomSeed: 1, basicAttack ?? BasicAttackSpell());
        return new CombatSimulation(setup);
    }

    [Fact]
    public void RequestMove_ToAdjacentEmptyTile_UpdatesPositionAndSpendsAp()
    {
        var sim = CreateSimulation();
        var player = sim.Entities[0];

        var moved = sim.RequestMove(player.Id, new GridPoint(4, 3));

        Assert.True(moved);
        Assert.Equal(new GridPoint(4, 3), player.Get<GridPosition>()!.Point);
        Assert.Equal(2, sim.PlayerActionPoints);
    }

    [Fact]
    public void RequestMove_ToNonAdjacentTile_Fails()
    {
        var sim = CreateSimulation();
        var player = sim.Entities[0];

        var moved = sim.RequestMove(player.Id, new GridPoint(6, 3));

        Assert.False(moved);
        Assert.Equal(3, sim.PlayerActionPoints);
    }

    [Fact]
    public void RequestAttack_UsesTheInjectedBasicAttacksCostAndEffects_NotAHardcodedValue()
    {
        // Deliberately different numbers from BasicAttackSpell()'s 5/1, so
        // this can only pass if RequestAttack actually reads CombatSetup's
        // BasicAttack rather than some leftover hardcoded Strike constant.
        var customAttack = new ComposedSpell(
            Name: "Custom Strike",
            Cost: 2,
            Tags: System.Array.Empty<Tag>(),
            Targeting: TargetingMode.Single,
            ChainAdditionalTargets: null,
            ChainFalloffPercent: null,
            Effects: new Effect[] { new DamageEffect(9) });

        var sim = CreateSimulation(enemyX: 4, enemyY: 3, basicAttack: customAttack);
        var player = sim.Entities[0];
        var goblin = sim.Entities[1];

        var attacked = sim.RequestAttack(player.Id, goblin.Id);

        Assert.True(attacked);
        Assert.Equal(11, goblin.Get<Health>()!.Current); // 20 - 9
        Assert.Equal(1, sim.PlayerActionPoints); // 3 - 2
    }

    [Fact]
    public void RequestAttack_OnAdjacentEnemy_DealsDamage_AndJournalsDamageDealt()
    {
        var sim = CreateSimulation(enemyX: 4, enemyY: 3);
        var player = sim.Entities[0];
        var goblin = sim.Entities[1];

        var attacked = sim.RequestAttack(player.Id, goblin.Id);

        Assert.True(attacked);
        Assert.Equal(15, goblin.Get<Health>()!.Current);
        Assert.Contains(sim.Journal.Entries, e => e.Type == EventType.DamageDealt);
    }

    [Fact]
    public void RequestAttack_OnNonAdjacentEnemy_Fails()
    {
        var sim = CreateSimulation(enemyX: 8, enemyY: 3);
        var player = sim.Entities[0];
        var goblin = sim.Entities[1];

        var attacked = sim.RequestAttack(player.Id, goblin.Id);

        Assert.False(attacked);
    }

    [Fact]
    public void ActionPoints_Exhausted_RejectsFurtherActions()
    {
        var sim = CreateSimulation(enemyX: 8, enemyY: 3);
        var player = sim.Entities[0];

        Assert.True(sim.RequestMove(player.Id, new GridPoint(4, 3)));
        Assert.True(sim.RequestMove(player.Id, new GridPoint(5, 3)));
        Assert.True(sim.RequestMove(player.Id, new GridPoint(6, 3)));
        Assert.Equal(0, sim.PlayerActionPoints);

        var fourthMove = sim.RequestMove(player.Id, new GridPoint(7, 3));

        Assert.False(fourthMove);
    }

    [Fact]
    public void EndPlayerTurn_ExecutesTheIntentDeclaredBeforeThePlayerActed_EvenIfThePlayerRepositions()
    {
        // Goblin starts adjacent, so its intent for this round is already
        // "Attack" before the player does anything this turn.
        var sim = CreateSimulation(enemyX: 4, enemyY: 3);
        var player = sim.Entities[0];
        var goblin = sim.Entities[1];
        var startingHealth = player.Get<Health>()!.Current;

        Assert.Equal(IntentKind.Attack, goblin.Get<Intent>()!.Kind);

        // The player moves away from the goblin during their own turn.
        Assert.True(sim.RequestMove(player.Id, new GridPoint(2, 3)));

        // The goblin's already-declared intent still resolves as
        // telegraphed, rather than being silently recomputed against the
        // player's new tile.
        sim.EndPlayerTurn();

        Assert.Equal(startingHealth - 5, player.Get<Health>()!.Current);
    }

    [Fact]
    public void DefeatingTheGoblin_EndsCombat_WithPlayerAsWinner()
    {
        var sim = CreateSimulation(enemyHealth: 5, enemyX: 4, enemyY: 3);
        var player = sim.Entities[0];
        var goblin = sim.Entities[1];

        sim.RequestAttack(player.Id, goblin.Id);

        Assert.True(sim.IsCombatOver);
        Assert.Equal(CombatWinner.Player, sim.Winner);
    }

    [Fact]
    public void EndPlayerTurn_TicksStatusDurations_AndExpiresAtZero()
    {
        var sim = CreateSimulation();
        var player = sim.Entities[0];
        player.Add(new Statuses());
        player.Get<Statuses>()!.Add(StatusType.Burn, stacks: 1, durationRounds: 1);

        sim.EndPlayerTurn();

        Assert.False(player.Get<Statuses>()!.Has(StatusType.Burn));
        Assert.Contains(sim.Journal.Entries, e => e.Type == EventType.StatusExpired);
    }

    [Fact]
    public void PlayerDefeated_EndsCombat_WithEnemiesAsWinner()
    {
        var sim = CreateSimulation(playerHealth: 5, enemyX: 4, enemyY: 3);
        var player = sim.Entities[0];

        // The goblin starts adjacent (Attack intent); ending the turn
        // immediately without the player acting resolves that attack.
        sim.EndPlayerTurn();

        Assert.True(player.Get<Health>()!.IsDefeated);
        Assert.True(sim.IsCombatOver);
        Assert.Equal(CombatWinner.Enemies, sim.Winner);
    }
}
