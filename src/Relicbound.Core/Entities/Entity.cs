using System;
using System.Collections.Generic;

namespace Relicbound.Core.Entities;

public sealed class Entity
{
    private readonly Dictionary<Type, IComponent> _components = new();

    public Entity(EntityId id, string name)
    {
        Id = id;
        Name = name;
    }

    public EntityId Id { get; }
    public string Name { get; }

    public void Add<T>(T component) where T : class, IComponent
    {
        _components[typeof(T)] = component;
    }

    public T? Get<T>() where T : class, IComponent
    {
        return _components.TryGetValue(typeof(T), out var component) ? (T)component : null;
    }

    public bool Has<T>() where T : class, IComponent
    {
        return _components.ContainsKey(typeof(T));
    }
}
