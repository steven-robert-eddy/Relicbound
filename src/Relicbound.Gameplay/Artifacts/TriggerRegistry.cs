using System;
using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Stats;

namespace Relicbound.Gameplay.Artifacts;

/// <remarks>
/// Owns the two guards Core's EffectResolver delegates to whatever
/// ITriggerSource it is given (docs/TECHNICAL_ARCHITECTURE.md section 7):
/// deterministic ordering by (equipment slot index, then artifact id), and a
/// per-round firing budget per (holder, artifact). Registration is decoupled
/// from the Equipment component on purpose -- Equipment (Milestone 2 issue
/// 2.6) will call Register/Unregister as artifacts are equipped, but nothing
/// here needs Equipment to exist to be correct and testable.
/// </remarks>
public sealed class TriggerRegistry : ITriggerSource
{
    private readonly List<Registration> _registrations = new();
    private readonly Dictionary<(EntityId Holder, string ArtifactId), int> _triggersUsedThisRound = new();

    public void Register(int slotIndex, ArtifactDefinition artifact, Entity holder)
    {
        _registrations.Add(new Registration(slotIndex, artifact, holder));
    }

    public void Unregister(Entity holder, string artifactId)
    {
        _registrations.RemoveAll(r => r.Holder.Id == holder.Id && r.Artifact.Id == artifactId);
    }

    public void ResetRoundBudgets()
    {
        _triggersUsedThisRound.Clear();
    }

    public IReadOnlyList<QueuedEffect> Match(IGameEvent gameEvent)
    {
        var (actor, recipient) = ExtractRoles(gameEvent);
        var result = new List<QueuedEffect>();

        var matching = _registrations
            .Where(r => r.Artifact.Trigger == gameEvent.Type)
            .OrderBy(r => r.SlotIndex)
            .ThenBy(r => r.Artifact.Id, StringComparer.Ordinal);

        foreach (var registration in matching)
        {
            var holderMatches = registration.Artifact.HolderRole switch
            {
                TriggerHolderRole.Actor => actor is not null && actor.Id == registration.Holder.Id,
                TriggerHolderRole.Recipient => recipient is not null && recipient.Id == registration.Holder.Id,
                _ => false,
            };

            if (!holderMatches) { continue; }

            var budgetKey = (registration.Holder.Id, registration.Artifact.Id);
            _triggersUsedThisRound.TryGetValue(budgetKey, out var used);
            if (used >= registration.Artifact.MaxTriggersPerRound) { continue; }
            _triggersUsedThisRound[budgetKey] = used + 1;

            var target = registration.Artifact.EffectTarget switch
            {
                EffectTargetSelector.Self => registration.Holder,
                EffectTargetSelector.EventActor => actor ?? registration.Holder,
                EffectTargetSelector.EventRecipient => recipient ?? registration.Holder,
                _ => registration.Holder,
            };

            var source = new ModifierSource(registration.Artifact.Id);
            foreach (var effectDefinition in registration.Artifact.Effects)
            {
                var effect = EffectDefinitionFactory.Create(effectDefinition, source);
                result.Add(new QueuedEffect(effect, new[] { target }));
            }
        }

        return result;
    }

    private static (Entity? Actor, Entity? Recipient) ExtractRoles(IGameEvent gameEvent) => gameEvent switch
    {
        DamageDealtEvent e => (e.Source, e.Target),
        HealedEvent e => (e.Source, e.Target),
        MovedEvent e => (null, e.Target),
        EntityDefeatedEvent e => (null, e.Target),
        PlayerDefeatedEvent e => (null, e.Target),
        StatusAppliedEvent e => (e.Source, e.Target),
        StatusStackedEvent e => (e.Source, e.Target),
        StatusExpiredEvent e => (null, e.Target),
        StatModifiedEvent e => (null, e.Target),
        _ => (null, null),
    };

    private sealed record Registration(int SlotIndex, ArtifactDefinition Artifact, Entity Holder);
}
