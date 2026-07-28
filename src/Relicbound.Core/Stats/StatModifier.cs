namespace Relicbound.Core.Stats;

/// <remarks>
/// For Flat, Value is an absolute amount. For Increased and More, Value is a
/// whole-number percentage (30 means +30%) — see StatBlock for why.
/// </remarks>
public readonly record struct StatModifier(StatType Stat, ModifierLayer Layer, int Value, ModifierSource Source);
