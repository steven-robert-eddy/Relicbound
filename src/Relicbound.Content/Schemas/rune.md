# Rune schema

One JSON file per rune under `Runes/`. Loaded and validated by
`RuneContentLoader` (`Relicbound.Content.Runes`) into
`Relicbound.Gameplay.Runes.RuneDefinition`. Every failure is reported at
once, same convention as `artifact.md`.

## Top-level fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | yes | globally unique |
| `name` | string | yes | |
| `namePrefix` | string | yes | prepended to the base spell's name in socket order -- see below |
| `addsTags` | string[] | no, default empty | tags this rune contributes to the composed spell |
| `costDelta` | int | no, default `0` | added to the base spell's cost; the composed total floors at 1 AP |
| `effects` | object[] | no, default empty | additional effects this rune contributes; same shape as artifact effects |
| `chainAdditionalTargets` | int | no | marks this as a Chain-modifying rune; positive int, extra targets beyond the first |
| `chainFalloffPercent` | int | required if `chainAdditionalTargets` is set | 1-100; each subsequent chain target takes this percent of the previous target's effect |

`chainAdditionalTargets` and `chainFalloffPercent` are a flat pair rather than
a generic "modifiers" list: there is exactly one modifier kind today (Chain).
A list is the right shape once a second one exists, not before.

## Composition (see `Relicbound.Gameplay.Spells.SpellComposer`)

Given a base spell and its socketed runes, in socket order:

- **Cost**: `spell.cost + sum(rune.costDelta)`, floored at 1.
- **Tags**: the union of the spell's tags and every rune's `addsTags`
  (deduplicated). Every effect in the composed spell carries the *full*
  union, not just whichever fragment contributed a given tag -- socketing a
  Flame Rune makes the whole spell count as Fire for any artifact that cares
  (`docs/GAME_DESIGN.md` section 7, `ArtifactDefinition.RequiredTag`).
- **Name**: each socketed rune's `namePrefix`, in socket order, followed by
  the base spell's name.
- **Targeting**: the base spell's `targeting`, unless a socketed rune sets
  `chainAdditionalTargets`, in which case it becomes `CHAIN`.
- **Effects**: the base spell's effects, then each rune's `effects`, in
  socket order, each compiled with the full composed tag set.

## Example: Bolt + Flame Rune + Chain Rune = Inferno Chain Bolt

```json
{
  "id": "rune_flame",
  "name": "Flame Rune",
  "namePrefix": "Inferno",
  "addsTags": ["Fire"],
  "costDelta": 0,
  "effects": [
    { "type": "APPLY_STATUS", "status": "BURN", "stacks": 2, "durationRounds": 3 }
  ]
}
```

```json
{
  "id": "rune_chain",
  "name": "Chain Rune",
  "namePrefix": "Chain",
  "addsTags": [],
  "costDelta": 1,
  "effects": [],
  "chainAdditionalTargets": 2,
  "chainFalloffPercent": 50
}
```

Socketed onto Bolt (cost 1) in that order: name **"Inferno Chain Bolt"**,
cost **2** AP (1 + 0 + 1), tags **Void, Fire**, targeting **CHAIN**, and every
compiled effect -- including Bolt's own damage -- carries the Fire tag.

> `docs/GAME_DESIGN.md`'s original sketch used a fractional `falloff: 0.5`.
> This schema uses an integer `chainFalloffPercent: 50` instead, matching the
> integer fixed-point convention `StatBlock` already established, for the
> same determinism reasons (`docs/CODING_STANDARDS.md`).
