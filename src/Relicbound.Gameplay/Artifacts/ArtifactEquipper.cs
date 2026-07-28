using Relicbound.Core.Entities;
using Relicbound.Core.Events;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// The event-publishing side of equipping, kept separate from Equipment
/// itself so the component stays plain data. Also the only thing that keeps
/// TriggerRegistry in sync with what is actually equipped.
/// </remarks>
public sealed class ArtifactEquipper
{
    private readonly IEventBus _eventBus;
    private readonly Journal _journal;
    private readonly TriggerRegistry _triggerRegistry;

    public ArtifactEquipper(IEventBus eventBus, Journal journal, TriggerRegistry triggerRegistry)
    {
        _eventBus = eventBus;
        _journal = journal;
        _triggerRegistry = triggerRegistry;
    }

    public void Equip(Entity holder, int slot, ArtifactDefinition artifact)
    {
        var equipment = holder.Get<Equipment>();
        if (equipment is null)
        {
            equipment = new Equipment();
            holder.Add(equipment);
        }

        var previous = equipment.Equip(slot, artifact);
        if (previous is not null)
        {
            _triggerRegistry.Unregister(holder, previous.Id);
            Publish(new ArtifactUnequippedEvent(holder, previous.Id));
        }

        _triggerRegistry.Register(slot, artifact, holder);
        Publish(new ArtifactEquippedEvent(holder, artifact.Id));
    }

    public void Unequip(Entity holder, int slot)
    {
        var previous = holder.Get<Equipment>()?.Unequip(slot);
        if (previous is null) { return; }

        _triggerRegistry.Unregister(holder, previous.Id);
        Publish(new ArtifactUnequippedEvent(holder, previous.Id));
    }

    private void Publish(IGameEvent gameEvent)
    {
        _journal.Record(gameEvent);
        _eventBus.Publish(gameEvent);
    }
}
