namespace Relicbound.Gameplay.Spells;

// Per docs/GAME_DESIGN.md section 4: single, chain (with falloff), area.
// A base spell declares its default; a Chain-modifying rune can override it
// at composition time -- see SpellComposer.
public enum TargetingMode
{
    Single,
    Chain,
    Area
}
