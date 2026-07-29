# Encounter schema

One JSON file per encounter under `Encounters/`. Loaded and validated by
`EncounterContentLoader` (`Relicbound.Content.Encounters`) into
`Relicbound.Gameplay.Encounters.EncounterDefinition`. Every failure is
reported at once, same convention as `artifact.md`.

## Top-level fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | yes | globally unique |
| `name` | string | yes | |
| `difficultyTier` | string | yes | one of `STANDARD`, `ELITE`, `BOSS` |
| `enemies` | object[] | yes, at least one | enemy composition, in the order they'll be placed |

## Enemy fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | yes | unique within the encounter, not globally |
| `name` | string | yes | |
| `health` | int | yes | positive |

## Who picks what

`difficultyTier` is how `Relicbound.Gameplay.Expeditions.ExpeditionMapGenerator`
matches an encounter to a node: Combat nodes draw from `STANDARD`, Elite nodes
from `ELITE`, the map's single Boss node from `BOSS`. The draw is a seeded pick
from whichever pool matches -- multiple encounters at the same tier is how a
node gets variety instead of always being the same fight.

## Deliberately not here yet

Grid layout, multi-phase boss behavior, and enemy stats beyond health (attack
patterns, resistances, abilities) aren't part of this schema. Every enemy
currently behaves like the combat sandbox's goblin
(`Relicbound.Gameplay.Combat.EnemyIntentPlanner`: approach and attack). Adding
real variety is later milestone work (4.5 for bosses in particular), not a
gap in this file.
