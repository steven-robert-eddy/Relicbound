using System;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Xunit;

namespace Relicbound.Gameplay.Tests.Artifacts;

/// <remarks>
/// The scenario docs/TECHNICAL_ARCHITECTURE.md section 7 warns about
/// verbatim: "Ember Heart applies Burn on damage; a Burn artifact deals
/// damage on Burn; Ember Heart triggers again." Two real, independently
/// authored artifacts, wired through the real TriggerRegistry and
/// EffectResolver -- not a fake ITriggerSource -- proving the depth cap
/// (not the per-round budget, which is set high enough here to stay out of
/// the way) is what breaks the cycle.
/// </remarks>
public class TriggerChainSafetyTests
{
    [Fact]
    public void CyclicArtifactPair_TerminatesAtMaxDepth_InsteadOfHanging()
    {
        var player = new Entity(new EntityId(1), "Player");
        player.Add(new Health(999));

        var goblin = new Entity(new EntityId(2), "Goblin");
        goblin.Add(new Health(9999));
        goblin.Add(new Statuses());

        // Pre-seed an existing Burn stack so every future application from
        // this cycle is a "stack" (STATUS_STACKED), never a fresh "apply"
        // (STATUS_APPLIED) -- otherwise the cycle would break on its own the
        // first time the event type flips, instead of exercising the guard.
        goblin.Get<Statuses>()!.Add(StatusType.Burn, stacks: 1, durationRounds: 99);

        var appliesBurnOnDamage = new ArtifactDefinition(
            Id: "cyclic_a",
            Name: "Cyclic A",
            Tags: Array.Empty<Tag>(),
            Trigger: EventType.DamageDealt,
            Effects: new[]
            {
                new EffectDefinition(EffectDefinitionType.ApplyStatus, Status: StatusType.Burn, Stacks: 1, DurationRounds: 99),
            },
            MaxTriggersPerRound: 999);

        var damagesOnStack = new ArtifactDefinition(
            Id: "cyclic_b",
            Name: "Cyclic B",
            Tags: Array.Empty<Tag>(),
            Trigger: EventType.StatusStacked,
            Effects: new[] { new EffectDefinition(EffectDefinitionType.Damage, Value: 1) },
            MaxTriggersPerRound: 999);

        var registry = new TriggerRegistry();
        registry.Register(0, appliesBurnOnDamage, player);
        registry.Register(1, damagesOnStack, player);

        var bus = new EventBus();
        var journal = new Journal();
        var resolver = new EffectResolver(bus, journal, registry);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(player, new[] { goblin }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new DamageEffect(1), context);

        var damageEventCount = journal.Entries.Count(e => e.Type == EventType.DamageDealt);
        var stackedEventCount = journal.Entries.Count(e => e.Type == EventType.StatusStacked);

        // depth 0,2,4,6,8 each produce one DAMAGE_DEALT; depth 1,3,5,7 each
        // produce one STATUS_STACKED. The depth-8 damage event still fires
        // (it's the effect *at* the cap) but is not itself re-matched.
        Assert.Equal(5, damageEventCount);
        Assert.Equal(4, stackedEventCount);
    }
}
