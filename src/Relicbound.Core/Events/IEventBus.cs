using System;

namespace Relicbound.Core.Events;

public interface IEventBus
{
    void Publish(IGameEvent gameEvent);
    IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent;
}
