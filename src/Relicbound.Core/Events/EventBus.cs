using System;
using System.Collections.Generic;

namespace Relicbound.Core.Events;

public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish(IGameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);

        if (!_handlers.TryGetValue(gameEvent.GetType(), out var handlers))
        {
            return;
        }

        // Snapshot before invoking: a handler that subscribes or
        // unsubscribes during dispatch must not mutate the list we iterate.
        foreach (var handler in handlers.ToArray())
        {
            handler.DynamicInvoke(gameEvent);
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var handlers))
        {
            handlers = new List<Delegate>();
            _handlers[type] = handlers;
        }

        handlers.Add(handler);
        return new Subscription(() => handlers.Remove(handler));
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public Subscription(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed) { return; }
            _disposed = true;
            _onDispose();
        }
    }
}
