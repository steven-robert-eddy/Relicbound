# Artifact schema

One JSON file per artifact under `Artifacts/`. Loaded and validated by
`ArtifactContentLoader` (`Relicbound.Content`) into
`Relicbound.Gameplay.Artifacts.ArtifactDefinition`. Every failure is reported
at once -- see `docs/TECHNICAL_ARCHITECTURE.md` section 10.

Enum-valued fields (`tags`, `trigger`, `holderRole`, `effectTarget`, effect
`type`/`status`/`stat`/`layer`) are matched case-insensitively and ignoring
underscores, so `"DAMAGE_DEALT"`, `"damage_dealt"`, and `"DamageDealt"` all
resolve to the same `EventType` member. This is what lets JSON stay
`UPPER_SNAKE` (matching `docs/GAME_DESIGN.md`'s examples) while the C# enums
stay idiomatic PascalCase.

## Top-level fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | yes | globally unique |
| `name` | string | yes | |
| `tags` | string[] | yes (may be empty) | one of the eleven `Tag` values |
| `trigger` | string | yes | an `EventType` member, e.g. `DAMAGE_DEALT` |
| `holderRole` | string | no, default `Actor` | `Actor` or `Recipient` -- which role the artifact's holder must play in the triggering event |
| `effectTarget` | string | no, default `EventRecipient` | `Self`, `EventActor`, or `EventRecipient` -- who the effects apply to |
| `maxTriggersPerRound` | int | no, default `1` | must be positive |
| `requiredTag` | string | no, default none | one of the eleven `Tag` values -- gates the trigger on the *firing effect itself* also carrying this tag (e.g. only Fire-tagged damage), not just the artifact's own identity tags above. See "Tag-gated triggers" below. |
| `effects` | object[] | yes (at least one) | see below |

## Effect entries

Each entry in `effects` has a `type` plus the fields that type requires:

| `type` | Required fields |
| --- | --- |
| `DAMAGE` | `value` (positive int) |
| `HEAL` | `value` (positive int) |
| `SHIELD` | `value` (positive int) |
| `APPLY_STATUS` | `status`, `stacks` (positive int), `durationRounds` (positive int) |
| `MODIFY_STAT` | `stat`, `layer`, `value` |
| `REVIVE` | `value` (1-100, percent of max health to revive at) |

## Tag-gated triggers

`requiredTag` is separate from `tags`. An artifact's own `tags` are identity --
Ember Heart is tagged `Fire, Spirit` whether or not the hit that triggers it
was itself Fire-flavored. `requiredTag`, when set, requires the *triggering
effect* to carry that tag: only a Fire-tagged attack would set off an artifact
with `requiredTag: "Fire"`. This is what makes a spell's composed tags
(`docs/GAME_DESIGN.md` section 7 -- socketing a Flame Rune makes the whole
spell count as Fire) actually mean something to an artifact that cares,
rather than just carrying the label. Omit it (the default) for an artifact
like Ember Heart or Phoenix Feather that reacts to any qualifying event
regardless of tag.

## Examples

Ember Heart uses only the defaults -- it triggers off its own holder's damage
(`holderRole: Actor`) and applies to whoever was hit (`effectTarget:
EventRecipient`, the default):

```json
{
  "id": "ember_heart",
  "name": "Ember Heart",
  "tags": ["Fire", "Spirit"],
  "trigger": "DAMAGE_DEALT",
  "effects": [
    { "type": "APPLY_STATUS", "status": "BURN", "stacks": 2, "durationRounds": 3 }
  ]
}
```

Phoenix Feather overrides both, because the artifact's own holder is the one
defeated, and the revive applies to that same holder, not to whoever dealt
the killing blow:

```json
{
  "id": "phoenix_feather",
  "name": "Phoenix Feather",
  "tags": ["Fire", "Spirit"],
  "trigger": "PLAYER_DEFEATED",
  "holderRole": "Recipient",
  "effectTarget": "Self",
  "effects": [
    { "type": "REVIVE", "value": 50 }
  ]
}
```

> The revive percent (50) and Ember Heart's Burn stacks/duration (2/3) are
> prototype defaults, not balance decisions -- tune freely once there's a
> reason to.
