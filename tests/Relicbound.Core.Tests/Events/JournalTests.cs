using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Xunit;

namespace Relicbound.Core.Tests.Events;

public class JournalTests
{
    [Fact]
    public void Record_KeepsEventsInOrder()
    {
        var journal = new Journal();
        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");

        journal.Record(new DamageDealtEvent(source, target, 5));
        journal.Record(new HealedEvent(source, target, 3));

        Assert.Equal(2, journal.Entries.Count);
        Assert.Equal(EventType.DamageDealt, journal.Entries[0].Type);
        Assert.Equal(EventType.Healed, journal.Entries[1].Type);
    }
}
