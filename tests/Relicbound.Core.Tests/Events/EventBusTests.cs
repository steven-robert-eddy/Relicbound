using System.Collections.Generic;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Xunit;

namespace Relicbound.Core.Tests.Events;

public class EventBusTests
{
    [Fact]
    public void Publish_DeliversToSubscriber()
    {
        var bus = new EventBus();
        var received = new List<DamageDealtEvent>();
        bus.Subscribe<DamageDealtEvent>(received.Add);

        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        bus.Publish(new DamageDealtEvent(source, target, 10));

        Assert.Single(received);
        Assert.Equal(10, received[0].Amount);
    }

    [Fact]
    public void Dispose_StopsFurtherDelivery()
    {
        var bus = new EventBus();
        var receivedCount = 0;
        var subscription = bus.Subscribe<DamageDealtEvent>(_ => receivedCount++);

        var source = new Entity(new EntityId(1), "Player");
        var target = new Entity(new EntityId(2), "Goblin");
        bus.Publish(new DamageDealtEvent(source, target, 10));
        subscription.Dispose();
        bus.Publish(new DamageDealtEvent(source, target, 10));

        Assert.Equal(1, receivedCount);
    }
}
