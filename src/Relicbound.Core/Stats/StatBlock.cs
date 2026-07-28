using System;
using System.Collections.Generic;
using System.Linq;
using Relicbound.Core.Entities;

namespace Relicbound.Core.Stats;

/// <remarks>
/// Resolution is Flat -> Increased -> More, using integer fixed-point math
/// (percentages as whole numbers) rather than floating point, so the same
/// build always computes the same number. See docs/CODING_STANDARDS.md.
/// </remarks>
public sealed class StatBlock : IComponent
{
    private readonly Dictionary<StatType, int> _baseValues = new();
    private readonly List<StatModifier> _modifiers = new();

    public void SetBase(StatType stat, int value)
    {
        _baseValues[stat] = value;
    }

    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
    }

    public void RemoveModifiersFrom(ModifierSource source)
    {
        _modifiers.RemoveAll(m => m.Source == source);
    }

    public int GetValue(StatType stat)
    {
        _baseValues.TryGetValue(stat, out var baseValue);

        var ordered = _modifiers
            .Where(m => m.Stat == stat)
            .OrderBy(m => m.Source.Id, StringComparer.Ordinal);

        var flatTotal = baseValue;
        var increasedSum = 0;
        var moreModifiers = new List<int>();

        foreach (var modifier in ordered)
        {
            switch (modifier.Layer)
            {
                case ModifierLayer.Flat:
                    flatTotal += modifier.Value;
                    break;
                case ModifierLayer.Increased:
                    increasedSum += modifier.Value;
                    break;
                case ModifierLayer.More:
                    moreModifiers.Add(modifier.Value);
                    break;
            }
        }

        var result = ApplyPercentage(flatTotal, increasedSum);

        foreach (var morePercent in moreModifiers)
        {
            result = ApplyPercentage(result, morePercent);
        }

        return result;
    }

    private static int ApplyPercentage(int value, int percent)
    {
        return (value * (100 + percent) + 50) / 100;
    }
}
