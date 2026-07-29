using System;
using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Effects;
using Relicbound.Core.Tags;
using Relicbound.Gameplay.Artifacts;
using Relicbound.Gameplay.Runes;

namespace Relicbound.Gameplay.Spells;

/// <remarks>
/// Deterministic merge of a base spell with its socketed runes, per
/// docs/GAME_DESIGN.md section 7. The canonical proof:
/// Bolt + Flame Rune + Chain Rune = Inferno Chain Bolt, costing 2 AP,
/// carrying the Fire tag through to every compiled effect.
/// </remarks>
public static class SpellComposer
{
    public static ComposedSpell Compose(SpellDefinition spell, IReadOnlyList<RuneDefinition> socketedRunes)
    {
        if (socketedRunes.Count > spell.Sockets)
        {
            throw new InvalidOperationException(
                $"'{spell.Id}' has {spell.Sockets} socket(s), but {socketedRunes.Count} rune(s) were socketed.");
        }

        var cost = Math.Max(1, spell.Cost + socketedRunes.Sum(r => r.CostDelta));
        var tags = ComposeTags(spell.Tags, socketedRunes);
        var name = ComposeName(spell.Name, socketedRunes);

        var chainRune = socketedRunes.FirstOrDefault(r => r.ChainAdditionalTargets is not null);
        var targeting = chainRune is not null ? TargetingMode.Chain : spell.Targeting;

        var source = new ModifierSource(spell.Id);
        var effects = new List<Effect>();

        foreach (var effectDefinition in spell.Effects)
        {
            effects.Add(EffectDefinitionFactory.Create(effectDefinition, source, tags));
        }

        foreach (var rune in socketedRunes)
        {
            foreach (var effectDefinition in rune.AddsEffects)
            {
                effects.Add(EffectDefinitionFactory.Create(effectDefinition, source, tags));
            }
        }

        return new ComposedSpell(
            name,
            cost,
            tags,
            targeting,
            chainRune?.ChainAdditionalTargets,
            chainRune?.ChainFalloffPercent,
            effects);
    }

    private static IReadOnlyList<Tag> ComposeTags(IReadOnlyList<Tag> baseTags, IReadOnlyList<RuneDefinition> runes)
    {
        var tags = new List<Tag>(baseTags);

        foreach (var rune in runes)
        {
            foreach (var tag in rune.AddsTags)
            {
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }
        }

        return tags;
    }

    private static string ComposeName(string baseName, IReadOnlyList<RuneDefinition> runes)
    {
        return string.Concat(runes.Select(r => r.NamePrefix + " ")) + baseName;
    }
}
