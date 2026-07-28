using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class EffectResolverTests
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

    private static EffectResolver CreateResolver(out Journal journal)
    {
        var bus = new EventBus();
        journal = new Journal();
        return new EffectResolver(bus, journal);
    }

    [Fact]
    public void DamageEffect_ReducesHealth_AndRecordsDamageDealtEvent()
    {
        var (source, target) = CreateCombatants();
        var resolver = CreateResolver(out var journal);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new DamageEffect(10), context);

        Assert.Equal(20, target.Get<Health>()!.Current);
        Assert.Contains(journal.Entries, e => e.Type == EventType.DamageDealt);
    }

    [Fact]
    public void DamageEffect_DefeatingThePlayer_RecordsPlayerDefeatedEvent()
    {
        var (source, target) = CreateCombatants();
        var resolver = CreateResolver(out var journal);
        var random = new SplitMix64RandomSource(1);

        // The goblin (source) attacks the player (target) so the
        // player-specific defeat event actually gets exercised.
        var context = new EffectContext(target, new[] { source }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new DamageEffect(999), context);

        Assert.Contains(journal.Entries, e => e.Type == EventType.EntityDefeated);
        Assert.Contains(journal.Entries, e => e.Type == EventType.PlayerDefeated);
    }

    [Fact]
    public void HealEffect_RestoresHealth_UpToMax()
    {
        var (source, target) = CreateCombatants();
        target.Get<Health>()!.ApplyDamage(20);
        var resolver = CreateResolver(out var journal);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new HealEffect(100), context);

        Assert.Equal(30, target.Get<Health>()!.Current);
    }

    [Fact]
    public void QueuedEffects_ExecuteAfterTheEffectThatQueuedThem()
    {
        var (source, target) = CreateCombatants();
        var resolver = CreateResolver(out var journal);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new QueuingDamageEffect(5, new HealEffect(2)), context);

        Assert.Equal(EventType.DamageDealt, journal.Entries[0].Type);
        Assert.Equal(EventType.Healed, journal.Entries[1].Type);
    }

    [Fact]
    public void Resolve_TerminatesAtMaxDepth_WhenAnEffectQueuesItselfForever()
    {
        var (source, target) = CreateCombatants();
        var resolver = CreateResolver(out var journal);
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(source, new[] { target }, random, depth: 0, EffectOrigin.Direct);

        resolver.Resolve(new SelfQueueingEffect(), context);

        var damageEventCount = journal.Entries.Count(e => e.Type == EventType.DamageDealt);
        Assert.Equal(EffectResolver.MaxTriggerDepth + 1, damageEventCount);
    }

    private sealed class QueuingDamageEffect : Effect
    {
        private readonly int _damage;
        private readonly Effect _followUp;

        public QueuingDamageEffect(int damage, Effect followUp)
        {
            _damage = damage;
            _followUp = followUp;
        }

        public override EffectResult Execute(EffectContext context)
        {
            var damageResult = new DamageEffect(_damage).Execute(context);
            return new EffectResult(
                damageResult.Events,
                new[] { new QueuedEffect(_followUp, context.Targets) });
        }
    }

    private sealed class SelfQueueingEffect : Effect
    {
        public override EffectResult Execute(EffectContext context)
        {
            var damageResult = new DamageEffect(1).Execute(context);
            return new EffectResult(
                damageResult.Events,
                new[] { new QueuedEffect(this, context.Targets) });
        }
    }
}
