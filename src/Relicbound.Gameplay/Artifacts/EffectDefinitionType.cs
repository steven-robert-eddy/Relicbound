namespace Relicbound.Gameplay.Artifacts;

// The closed, engine-known set of effect kinds content can reference. Growing
// this list means adding a Core effect and a case in EffectDefinitionFactory
// -- adding an artifact that reuses an existing kind never touches code.
public enum EffectDefinitionType
{
    Damage,
    Heal,
    ApplyStatus,
    ModifyStat
}
