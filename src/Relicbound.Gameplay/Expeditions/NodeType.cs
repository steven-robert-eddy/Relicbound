namespace Relicbound.Gameplay.Expeditions;

// docs/GAME_DESIGN.md section 8. Start and Boss are structural -- exactly one
// of each per map. Combat, Elite, Treasure, Event, and Merchant are the
// choices ExpeditionMapGenerator scatters across the floors between them.
public enum NodeType
{
    Start,
    Combat,
    Elite,
    Treasure,
    Event,
    Merchant,
    Boss
}
