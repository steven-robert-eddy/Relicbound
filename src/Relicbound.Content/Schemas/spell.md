# Spell schema

One JSON file per base spell under `Spells/`. Loaded and validated by
`SpellContentLoader` (`Relicbound.Content.Spells`) into
`Relicbound.Gameplay.Spells.SpellDefinition`. Every failure is reported at
once, same convention as `artifact.md` -- see `docs/TECHNICAL_ARCHITECTURE.md`
section 10.

Effect entries share the exact same shape and validation rules as artifact
effects (`EffectDefinitionJsonParsing`, used by both loaders) -- see
`artifact.md`'s "Effect entries" section for the full table, now including
`SHIELD` (`value`, positive int).

## Top-level fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | yes | globally unique |
| `name` | string | yes | |
| `tags` | string[] | yes (may be empty) | one of the eleven `Tag` values |
| `cost` | int | yes | positive; the base AP cost before any runes |
| `range` | int | yes | 0 or more; 0 means self-target only |
| `sockets` | int | yes | 0 or more; how many runes can be socketed |
| `targeting` | string | no, default `SINGLE` | `SINGLE`, `CHAIN`, or `AREA` |
| `effects` | object[] | yes (at least one) | see `artifact.md` |

## A spell is never cast directly from this file

`SpellDefinition` is the *base* spell -- what socketing runes actually
produces is a `ComposedSpell`, built by `SpellComposer.Compose`, not loaded
from JSON itself. See `rune.md` for how runes attach.

## Examples

```json
{
  "id": "bolt",
  "name": "Bolt",
  "tags": ["Void"],
  "cost": 1,
  "range": 4,
  "sockets": 2,
  "targeting": "SINGLE",
  "effects": [
    { "type": "DAMAGE", "value": 6 }
  ]
}
```

```json
{
  "id": "strike",
  "name": "Strike",
  "tags": [],
  "cost": 1,
  "range": 1,
  "sockets": 0,
  "targeting": "SINGLE",
  "effects": [
    { "type": "DAMAGE", "value": 5 }
  ]
}
```

> Strike's numbers (5 damage, 1 AP) match the values `CombatSimulation`
> already hardcodes for the player's and enemies' basic attack. This file
> documents that same attack as data; `CombatSimulation` is not yet wired to
> read it -- rewiring `RequestAttack` to cast a real `ComposedSpell` is a
> follow-up, not part of this pass.
