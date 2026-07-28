using Relicbound.Core.Events;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

public class EquipmentTests
{
    private static ArtifactDefinition MakeArtifact(string id) => new(
        Id: id,
        Name: id,
        Tags: System.Array.Empty<Tag>(),
        Trigger: EventType.DamageDealt,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Heal, Value: 1) });

    [Fact]
    public void Equip_ThenGet_ReturnsTheEquippedArtifact()
    {
        var equipment = new Equipment();

        equipment.Equip(0, MakeArtifact("a"));

        Assert.Equal("a", equipment.Get(0)!.Id);
    }

    [Fact]
    public void Equip_OverAnOccupiedSlot_ReturnsThePreviousArtifact()
    {
        var equipment = new Equipment();
        equipment.Equip(0, MakeArtifact("a"));

        var previous = equipment.Equip(0, MakeArtifact("b"));

        Assert.Equal("a", previous!.Id);
        Assert.Equal("b", equipment.Get(0)!.Id);
    }

    [Fact]
    public void Unequip_ClearsTheSlot_AndReturnsWhatWasThere()
    {
        var equipment = new Equipment();
        equipment.Equip(0, MakeArtifact("a"));

        var removed = equipment.Unequip(0);

        Assert.Equal("a", removed!.Id);
        Assert.Null(equipment.Get(0));
    }

    [Fact]
    public void Equip_OutOfRangeSlot_Throws()
    {
        var equipment = new Equipment();

        Assert.Throws<System.ArgumentOutOfRangeException>(() => equipment.Equip(Equipment.SlotCount, MakeArtifact("a")));
    }
}
