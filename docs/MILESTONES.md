# Relicbound — GitHub Milestones

Execution breakdown of `PROTOTYPE_ROADMAP.md`, sized for GitHub issues. Each task
is meant to be a single issue that one person or one agent can finish and prove.

`docs/PROTOTYPE_ROADMAP.md` says *why* the order is what it is. This file is
*what to create*.

---

## Milestone 1 — The Spark

> A combat simulation works.

| # | Issue | Done when |
| --- | --- | --- |
| 1.1 | Entity and component model | `Entity` with a component bag; `Health`, `StatBlock`, `GridPosition`, `Statuses`, `TurnResources`; unit tested |
| 1.2 | Event bus and event catalogue | Publish/subscribe with typed events; the catalogue from the architecture doc; tested |
| 1.3 | Journal | Ordered event record; renders to readable text; is the single source for log and animation |
| 1.4 | Effect base and resolution loop | `Effect`, `EffectContext`, `EffectResult`; iterative queue; **all four recursion guards** |
| 1.5 | Core effects | `DamageEffect`, `HealEffect`, `ApplyStatusEffect`, `ModifyStatEffect`, `MoveEffect` |
| 1.6 | Stat system | Flat/Increased/More layers, deterministic ordering, explainable results |
| 1.7 | Seeded random source | `IRandomSource`, split streams, serializable state |
| 1.8 | Turn resource model | `ITurnResourceModel` + `ActionPointModel` at 3 AP |
| 1.9 | Grid and movement rules | Tile coordinates, occupancy, range and pathing, AP cost per tile |
| 1.10 | Combat simulation loop | Rounds, turns, status ticks, victory/defeat; runs headlessly |
| 1.11 | Basic enemy behaviour | Approach and attack; declares intent before the player's turn |
| 1.12 | Godot combat scene | Grid render, two squares, click to move, click to attack |
| 1.13 | Combat log UI | Renders the Journal live |
| 1.14 | CI green with real tests | `dotnet test` passes on CI with no engine installed |

**Milestone demo:** player moves, attacks a goblin, goblin takes damage, an event
fires, combat resolves. No graphics required for the underlying proof.

---

## Milestone 2 — The First Relic

> The build system exists.

| # | Issue | Done when |
| --- | --- | --- |
| 2.1 | Tag system | Eleven tags; tag sets on entities, effects, and definitions; matching |
| 2.2 | Trigger definitions | Trigger types matched against events |
| 2.3 | Trigger registry and ordering | Deterministic ordering by slot then id; per-round budgets |
| 2.4 | Artifact definition and schema | `tags + trigger + effects` as JSON; documented schema |
| 2.5 | Content loader and validation | Embedded JSON; validate all, report every failure at once |
| 2.6 | Equipment component | Three artifact slots; equip and unequip publish events |
| 2.7 | Ember Heart | Pure data. Applies Burn on `DAMAGE_DEALT` |
| 2.8 | Burn status | Damage over time, stacking rules, expiry |
| 2.9 | Phoenix Feather | Pure data. Revives on `PLAYER_DEFEATED` |
| 2.10 | Revive effect | Restores at threshold; consumes the artifact |
| 2.11 | Trigger chain safety tests | A deliberately cyclic pair terminates instead of hanging |
| 2.12 | Artifact UI | Equipped artifacts visible; triggers appear in the log |

**Milestone demo:** equip Ember Heart, and combat changes — with no code written
for Ember Heart specifically.

---

## Milestone 3 — The First Spell

> Spell customization works.

| # | Issue | Done when |
| --- | --- | --- |
| 3.1 | Spell definition and schema | Cost, range, targeting, sockets, effects |
| 3.2 | Rune definition and schema | Tag additions, cost delta, effects, modifiers, name prefix |
| 3.3 | Spell composition | Deterministic merge of base plus runes |
| 3.4 | Composed cost and tags | Cost floors at 1; tags are the union — Fire runes make the spell Fire |
| 3.5 | Name generation | Prefixes in socket order, then base name |
| 3.6 | Targeting modes | Single, chain with falloff, area |
| 3.7 | Base spells | Strike, Bolt, Shield |
| 3.8 | First runes | Flame Rune, Chain Rune |
| 3.9 | Rune socketing UI | Socket, unsocket, preview before committing |
| 3.10 | Composition tests | Cost, tags, effects, naming, and interaction with artifacts |

**Milestone demo:** `Bolt + Flame Rune + Chain Rune` = **Inferno Chain Bolt** —
and because it now carries the Fire tag, Ember Heart triggers off it.

---

## Milestone 4 — The First Expedition

> A complete playable loop.

| # | Issue | Done when |
| --- | --- | --- |
| 4.1 | Expedition map generation | Seeded node graph in the shape from the design doc |
| 4.2 | Node types | Combat, elite, treasure, event, merchant, boss |
| 4.3 | Expedition state machine | Enter node, resolve, advance, complete or die |
| 4.4 | Reward tables | Seeded artifact and rune drops |
| 4.5 | Boss encounter | Multi-phase fight |
| 4.6 | Workshop scene | Loadout selection with Resonance preview |
| 4.7 | Archive system | Permanent record of identified artifacts and discovered synergies |
| 4.8 | Synergy discovery | `SYNERGY_DISCOVERED` fires and writes to the Archive |
| 4.9 | Archive UI | Known entries plus visible, countable `???` slots |
| 4.10 | Vault | Three slots; deposit on boss clear |
| 4.11 | Resonance | Scaling driven by carried artifacts; raises both difficulty and rewards |
| 4.12 | Save and load | Versioned profile and run saves |
| 4.13 | Full loop integration | Workshop → expedition → combat → return or death → Archive |

**Milestone demo:** a complete run, and the next one starts differently because
of what you learned and what you chose to carry.

---

## Labels

```
milestone-1  milestone-2  milestone-3  milestone-4
core         gameplay     content      godot       ui
tests        docs         architecture
```

## Definition of done (every issue)

- [ ] `dotnet build` clean, no new warnings
- [ ] `dotnet test` passes with no engine installed
- [ ] New systems have tests covering their rules
- [ ] No Godot dependency added to `Core` or `Gameplay`
- [ ] New content is data, not code
- [ ] Docs updated if architecture changed
