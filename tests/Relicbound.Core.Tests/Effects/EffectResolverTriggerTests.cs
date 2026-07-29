using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class EffectResolverTriggerTests
{
    private static (Entity Source, Entity Target) CreateCombatants()
    {
        var source = new Entity(new EntityId(1), "Player");
        source.Add(new PlayerControlled());
        source.Add(new Health(30));

        var target = new Entity(new EntityId(2), "Goblin");
        target.Add(new Health(30));

        return (source, target);
    }

    [Fact]
    public void Resolve_WithTriggerSource_EnqueuesMatchedEffectAfterTheEventThatCausedIt()
    {
        var (source, target) = CreateCombatants();
        var bus = new EventBus();
        var journal = new Journal();
        var triggerSource = new OneShotTriggerSource();
        var resolver = new EffectResolver(bus, journal, triggerSource);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new DamageEffect(5), context);

        var damageEvents = journal.Entries.Where(e => e.Type == EventType.DamageDealt).ToList();
        Assert.Equal(2, damageEvents.Count);
        Assert.Equal(2, triggerSource.MatchCalls);
    }

    [Fact]
    public void Resolve_WithAlwaysMatchingTriggerSource_TerminatesAtMaxDepth()
    {
        var (source, target) = CreateCombatants();
        var bus = new EventBus();
        var journal = new Journal();
        var triggerSource = new AlwaysMatchingTriggerSource();
        var resolver = new EffectResolver(bus, journal, triggerSource);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new DamageEffect(1), context);

        var damageEventCount = journal.Entries.Count(e => e.Type == EventType.DamageDealt);
        Assert.Equal(EffectResolver.MaxTriggerDepth + 1, damageEventCount);
    }

    private sealed class OneShotTriggerSource : ITriggerSource
    {
        public int MatchCalls { get; private set; }

        public IReadOnlyList<TriggerActivation> Match(IGameEvent gameEvent, IReadOnlyCollection<Relicbound.Core.Tags.Tag>? effectTags = null)
        {
            MatchCalls++;

            if (MatchCalls > 1 || gameEvent is not DamageDealtEvent damageDealt)
            {
                return System.Array.Empty<TriggerActivation>();
            }

            var queued = new QueuedEffect(new DamageEffect(1), new[] { damageDealt.Target });
            return new[] { new TriggerActivation(new ArtifactTriggeredEvent(damageDealt.Source, "fake"), new[] { queued }) };
        }
    }

    private sealed class AlwaysMatchingTriggerSource : ITriggerSource
    {
        public IReadOnlyList<TriggerActivation> Match(IGameEvent gameEvent, IReadOnlyCollection<Relicbound.Core.Tags.Tag>? effectTags = null)
        {
            if (gameEvent is not DamageDealtEvent damageDealt)
            {
                return System.Array.Empty<TriggerActivation>();
            }

            var queued = new QueuedEffect(new DamageEffect(1), new[] { damageDealt.Target });
            return new[] { new TriggerActivation(new ArtifactTriggeredEvent(damageDealt.Source, "fake"), new[] { queued }) };
        }
    }
}
