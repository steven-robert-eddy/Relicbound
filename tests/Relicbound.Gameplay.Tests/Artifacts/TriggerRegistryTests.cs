using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Stats;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

public class TriggerRegistryTests
{
    private static ArtifactDefinition EmberHeartLike(int maxTriggersPerRound = 1) => new(
        Id: "ember_heart",
        Name: "Ember Heart",
        Tags: new[] { Tag.Fire, Tag.Spirit },
        Trigger: EventType.DamageDealt,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.ApplyStatus, Status: StatusType.Burn, Stacks: 2, DurationRounds: 3) },
        MaxTriggersPerRound: maxTriggersPerRound);

    private static ArtifactDefinition PhoenixFeatherLike() => new(
        Id: "phoenix_feather",
        Name: "Phoenix Feather",
        Tags: new[] { Tag.Fire, Tag.Spirit },
        Trigger: EventType.PlayerDefeated,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Heal, Value: 1) },
        HolderRole: TriggerHolderRole.Recipient,
        EffectTarget: EffectTargetSelector.Self);

    [Fact]
    public void Match_WhenHolderIsTheActor_ReturnsEffectTargetingTheRecipient()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();
        registry.Register(slotIndex: 0, EmberHeartLike(), holder);

        var matches = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        var activation = Assert.Single(matches);
        var queued = Assert.Single(activation.Effects);
        Assert.Same(recipient, Assert.Single(queued.Targets));
    }

    [Fact]
    public void Match_WhenHolderIsNotTheActor_ReturnsNothing()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var attacker = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();
        registry.Register(slotIndex: 0, EmberHeartLike(), holder);

        // The goblin dealt the damage, not the artifact's holder -- Ember
        // Heart should not proc off damage it didn't cause.
        var matches = registry.Match(new DamageDealtEvent(attacker, holder, 10));

        Assert.Empty(matches);
    }

    [Fact]
    public void Match_RecipientRoleWithSelfTarget_TargetsTheHolderItself()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var registry = new TriggerRegistry();
        registry.Register(slotIndex: 0, PhoenixFeatherLike(), holder);

        var matches = registry.Match(new PlayerDefeatedEvent(holder));

        var activation = Assert.Single(matches);
        var queued = Assert.Single(activation.Effects);
        Assert.Same(holder, Assert.Single(queued.Targets));
    }

    [Fact]
    public void Match_OrdersBySlotIndex_RegardlessOfRegistrationOrder()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();

        var slotOneArtifact = new ArtifactDefinition(
            Id: "slot_one_artifact",
            Name: "Slot One Artifact",
            Tags: System.Array.Empty<Tag>(),
            Trigger: EventType.DamageDealt,
            Effects: new[] { new EffectDefinition(EffectDefinitionType.Heal, Value: 7) });

        // Register slot 1 first, to prove ordering comes from SlotIndex, not
        // registration order.
        registry.Register(slotIndex: 1, slotOneArtifact, holder);
        registry.Register(slotIndex: 0, EmberHeartLike(), holder);

        var matches = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        Assert.Equal(2, matches.Count);
        Assert.IsType<Relicbound.Core.Effects.ApplyStatusEffect>(Assert.Single(matches[0].Effects).Effect);
        Assert.IsType<Relicbound.Core.Effects.HealEffect>(Assert.Single(matches[1].Effects).Effect);
    }

    [Fact]
    public void Match_ExceedingMaxTriggersPerRound_StopsFiring_UntilBudgetReset()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();
        registry.Register(slotIndex: 0, EmberHeartLike(maxTriggersPerRound: 1), holder);

        var firstMatch = registry.Match(new DamageDealtEvent(holder, recipient, 10));
        var secondMatch = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        Assert.Single(firstMatch);
        Assert.Empty(secondMatch);

        registry.ResetRoundBudgets();
        var afterReset = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        Assert.Single(afterReset);
    }

    [Fact]
    public void Match_ReturnsAnArtifactTriggeredAnnouncement_IdentifyingTheArtifact()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();
        registry.Register(slotIndex: 0, EmberHeartLike(), holder);

        var matches = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        var activation = Assert.Single(matches);
        var announcement = Assert.IsType<ArtifactTriggeredEvent>(activation.AnnouncementEvent);
        Assert.Same(holder, announcement.Holder);
        Assert.Equal("ember_heart", announcement.ArtifactId);
    }

    [Fact]
    public void Match_AfterUnregister_NoLongerFires()
    {
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var registry = new TriggerRegistry();
        var artifact = EmberHeartLike();
        registry.Register(slotIndex: 0, artifact, holder);

        registry.Unregister(holder, artifact.Id);
        var matches = registry.Match(new DamageDealtEvent(holder, recipient, 10));

        Assert.Empty(matches);
    }
}
