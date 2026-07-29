using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Runes;
using Relicbound.Gameplay.Spells;
using Xunit;

namespace Relicbound.Gameplay.Tests.Spells;

/// <remarks>
/// The Milestone 3 headline proof (docs/PROTOTYPE_ROADMAP.md): a composed
/// spell's Fire tag actually gates a Fire-requiring artifact, not just
/// carries the label. Deliberately not testing Ember Heart itself -- it has
/// no RequiredTag, matching docs/GAME_DESIGN.md's canonical example -- so
/// this uses a fabricated Fire-gated test artifact to prove the mechanism
/// end to end through the real EffectResolver and TriggerRegistry.
/// </remarks>
public class SpellArtifactIntegrationTests
{
    private static SpellDefinition Bolt() => new(
        Id: "bolt",
        Name: "Bolt",
        Tags: new[] { Tag.Void },
        Cost: 1,
        Range: 4,
        Sockets: 2,
        Targeting: TargetingMode.Single,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Damage, Value: 6) });

    private static RuneDefinition FlameRune() => new(
        Id: "rune_flame",
        Name: "Flame Rune",
        NamePrefix: "Inferno",
        AddsTags: new[] { Tag.Fire },
        CostDelta: 0,
        AddsEffects: new[]
        {
            new EffectDefinition(EffectDefinitionType.ApplyStatus, Status: StatusType.Burn, Stacks: 2, DurationRounds: 3),
        });

    private static ArtifactDefinition FireGatedArtifact() => new(
        Id: "fire_gated_test_artifact",
        Name: "Fire-Gated Test Artifact",
        Tags: new[] { Tag.Fire },
        Trigger: EventType.DamageDealt,
        Effects: new[] { new EffectDefinition(EffectDefinitionType.Heal, Value: 3) },
        RequiredTag: Tag.Fire);

    [Fact]
    public void FireTaggedComposedSpell_TriggersFireGatedArtifact()
    {
        var caster = new Entity(new EntityId(1), "Player");
        caster.Add(new Health(30));
        caster.Get<Health>()!.ApplyDamage(10); // 20/30, so a heal is visible
        var target = new Entity(new EntityId(2), "Goblin");
        target.Add(new Health(30));

        var registry = new TriggerRegistry();
        registry.Register(0, FireGatedArtifact(), caster);

        var bus = new EventBus();
        var journal = new Journal();
        var resolver = new EffectResolver(bus, journal, registry);
        var random = new SplitMix64RandomSource(1);

        var composed = SpellComposer.Compose(Bolt(), new[] { FlameRune() });
        var damageEffect = composed.Effects.OfType<DamageEffect>().Single();

        resolver.Resolve(
            damageEffect, new EffectContext(caster, new[] { target }, random, depth: 0, EffectOrigin.Direct));

        Assert.Contains(journal.Entries, e => e.Type == EventType.ArtifactTriggered);
        Assert.Equal(23, caster.Get<Health>()!.Current);
    }

    [Fact]
    public void PlainNonFireDamage_DoesNotTriggerFireGatedArtifact()
    {
        var caster = new Entity(new EntityId(1), "Player");
        caster.Add(new Health(30));
        caster.Get<Health>()!.ApplyDamage(10); // 20/30, so a missed heal is visible too
        var target = new Entity(new EntityId(2), "Goblin");
        target.Add(new Health(30));

        var registry = new TriggerRegistry();
        registry.Register(0, FireGatedArtifact(), caster);

        var bus = new EventBus();
        var journal = new Journal();
        var resolver = new EffectResolver(bus, journal, registry);
        var random = new SplitMix64RandomSource(1);

        // No Flame Rune socketed -- Bolt stays Void-tagged only.
        var composed = SpellComposer.Compose(Bolt(), System.Array.Empty<RuneDefinition>());
        var damageEffect = composed.Effects.OfType<DamageEffect>().Single();

        resolver.Resolve(
            damageEffect, new EffectContext(caster, new[] { target }, random, depth: 0, EffectOrigin.Direct));

        Assert.DoesNotContain(journal.Entries, e => e.Type == EventType.ArtifactTriggered);
        Assert.Equal(20, caster.Get<Health>()!.Current);
    }
}
