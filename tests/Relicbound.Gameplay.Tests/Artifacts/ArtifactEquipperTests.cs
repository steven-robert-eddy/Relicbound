using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

public class ArtifactEquipperTests
{
    private static ArtifactDefinition MakeArtifact(string id) => new(
        Id: id,
        Name: id,
        Tags: System.Array.Empty<Tag>(),
        Trigger: EventType.DamageDealt,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Heal, Value: 1) });

    [Fact]
    public void Equip_PublishesArtifactEquippedEvent_AndRegistersWithTheTriggerRegistry()
    {
        var bus = new EventBus();
        var journal = new Journal();
        var registry = new TriggerRegistry();
        var equipper = new ArtifactEquipper(bus, journal, registry);
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        var artifact = MakeArtifact("ember_heart");

        equipper.Equip(holder, 0, artifact);

        Assert.Contains(journal.Entries, e => e is ArtifactEquippedEvent equipped && equipped.ArtifactId == "ember_heart");
        Assert.Single(registry.Match(new DamageDealtEvent(holder, recipient, 1)));
    }

    [Fact]
    public void Unequip_PublishesArtifactUnequippedEvent_AndStopsItFromFiring()
    {
        var bus = new EventBus();
        var journal = new Journal();
        var registry = new TriggerRegistry();
        var equipper = new ArtifactEquipper(bus, journal, registry);
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        equipper.Equip(holder, 0, MakeArtifact("ember_heart"));

        equipper.Unequip(holder, 0);

        Assert.Contains(journal.Entries, e => e is ArtifactUnequippedEvent unequipped && unequipped.ArtifactId == "ember_heart");
        Assert.Empty(registry.Match(new DamageDealtEvent(holder, recipient, 1)));
    }

    [Fact]
    public void Equip_OverAnOccupiedSlot_UnregistersThePreviousArtifact()
    {
        var bus = new EventBus();
        var journal = new Journal();
        var registry = new TriggerRegistry();
        var equipper = new ArtifactEquipper(bus, journal, registry);
        var holder = new Entity(new EntityId(1), "Player");
        var recipient = new Entity(new EntityId(2), "Goblin");
        equipper.Equip(holder, 0, MakeArtifact("first"));

        equipper.Equip(holder, 0, MakeArtifact("second"));

        var matches = registry.Match(new DamageDealtEvent(holder, recipient, 1));
        var activation = Assert.Single(matches);
        Assert.Equal("second", ((ArtifactTriggeredEvent)activation.AnnouncementEvent).ArtifactId);
    }
}
