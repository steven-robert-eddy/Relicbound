# Relicbound — Prototype Roadmap

Four milestones. Each one proves something on its own, and each one is playable
or testable before the next begins.

The ordering is deliberate: systems first, then the things built on them, then
the loop that contains them. Resist building ahead — a half-finished expedition
system on top of an unproven effect system is how prototypes die.

**The prototype is finished when it answers one question:**

> Is exploring, discovering artifacts, and creating builds fun?

Not "is it polished," not "is it balanced." Just that.

---

## Milestone 1 — The Spark

**Goal:** a playable combat sandbox. The simulation works and you can watch it
work.

### Build order

1. **Entity + components** — `Entity`, `Health`, `StatBlock`, `GridPosition`,
   `Statuses`, `TurnResources`
2. **Event bus** — publish/subscribe, the event catalogue, the Journal
3. **Effects** — `Effect`, `EffectContext`, `EffectResult`, and the iterative
   resolution loop with its recursion guards
4. **Stats** — base values plus Flat/Increased/More modifier layers
5. **Turn model** — `ITurnResourceModel` and `ActionPointModel` at 3 AP
6. **Combat simulation** — round loop, turn order, victory and defeat
7. **Minimal Godot view** — a grid, two coloured squares, an action bar, a
   combat log

### Success criteria

- [ ] Player moves on a grid, spending AP
- [ ] Player attacks an enemy; the enemy takes damage
- [ ] `DAMAGE_DEALT` fires and is recorded in the Journal
- [ ] Combat resolves to victory or defeat
- [ ] The combat log reads clearly enough to follow what happened
- [ ] `dotnet test` covers damage, effects, and AP spending — and passes with no
      engine installed

### Visual target

Do not start with art. This is the target, and it is enough:

```
+----------------+
|                |
|    E           |
|                |
|        P       |
|                |
+----------------+

P = Player    E = Enemy
```

### Non-goals

Artifacts · spells beyond a basic attack · content files · expeditions ·
menus · art · audio

---

## Milestone 2 — The First Relic

**Goal:** the build system exists. Equipping something changes how combat plays.

### Build order

1. **Tag system** — the eleven tags, and tag matching
2. **Trigger system** — trigger definitions, matching against events,
   deterministic ordering, per-round budgets
3. **Artifact definitions** — `tags + trigger + effects`, loaded from JSON
4. **Equipment component** — three artifact slots
5. **Content loader** — JSON parsing plus fail-loudly validation
6. **First artifacts** — Ember Heart and Phoenix Feather, as data
7. **UI** — show equipped artifacts; log when one triggers

### Success criteria

- [ ] Equipping **Ember Heart** causes attacks to apply Burn
- [ ] Equipping **Phoenix Feather** revives the player at zero health
- [ ] Both are pure JSON — no artifact-specific code anywhere
- [ ] `ARTIFACT_TRIGGERED` appears in the combat log
- [ ] A trigger chain terminates at max depth instead of hanging **(test this
      explicitly — see `TECHNICAL_ARCHITECTURE.md` §7)**
- [ ] Adding a third artifact requires zero code changes

The last two are the real milestone. Everything else is scaffolding around them.

### Non-goals

Spells beyond the basics · runes · expeditions · the Workshop · the Archive

---

## Milestone 3 — The First Spell

**Goal:** spell customization works. The player composes something the designer
never wrote down.

### Build order

1. **Spell definitions** — base spell, cost, range, targeting, sockets
2. **Rune definitions** — tag additions, cost deltas, effect additions,
   modifiers, name prefixes
3. **Composition** — deterministic merge of base plus runes into a resolved spell
4. **Name generation** — rune prefixes in socket order, then the base name
5. **Targeting** — single, chain, area
6. **UI** — socket runes, preview the composed spell before committing

### Success criteria

- [ ] `Bolt + Flame Rune + Chain Rune` produces **Inferno Chain Bolt**
- [ ] The composed spell deals damage, chains to additional targets, and applies
      Burn
- [ ] Composed cost is base plus deltas, floored at 1
- [ ] The composed spell gains the **Fire** tag, and therefore triggers
      Fire-tagged artifacts — *this is the moment two systems meet, and the first
      real proof of the design*
- [ ] Composition is fully tested: cost, tags, effects, naming

### Non-goals

Expeditions · the Workshop · progression · balance

---

## Milestone 4 — The First Expedition

**Goal:** a complete playable loop. Start to death or victory, and back again.

### Build order

1. **Expedition map** — seeded node graph generation
2. **Node types** — combat, elite, treasure, event, merchant, boss
3. **Rewards** — artifact and rune drops from a seeded pool
4. **Boss encounter** — a fight with a second phase
5. **The Workshop** — loadout selection, Archive browsing, Experiment sandbox
6. **The Archive** — record discovered artifacts and synergies permanently
7. **The Vault** — three slots, deposit on boss clear
8. **Resonance** — difficulty scaling driven by carried artifacts
9. **Save/load** — profile and run persistence

### Success criteria

- [ ] A full run is playable start to finish
- [ ] Death loses carried relics; the Archive survives
- [ ] Discovering `Fire + Spirit` writes a permanent Archive entry
- [ ] Vaulted artifacts can be carried into the next expedition
- [ ] Carrying them raises Resonance, and the expedition is measurably harder
- [ ] Two runs from the same seed play identically; different seeds do not

### Non-goals

Art · audio · meta-narrative · balance passes · anything from the scope
protection list

---

## After the prototype

Only once the loop is proven fun, and in roughly this order:

1. **Content volume** — the systems are worthless without enough artifacts to
   combine. This is the first thing to do, not the last.
2. **Balance** — using the simulation harness, not intuition
3. **Presentation** — art, animation, audio, game feel
4. **Descent ladder** — the optional mastery difficulty track
5. **Broader expeditions** — more biomes, node types, bosses

If the loop is *not* fun, none of the above will fix it. Change the loop.

---

## Working rules

- **One milestone at a time.** Finish and prove it before starting the next.
- **Tests land with the system**, not in a cleanup pass afterward.
- **Content is data.** If new content needs code, fix the system instead.
- **When a milestone reveals a design problem**, update `GAME_DESIGN.md`. The
  docs are the plan of record, not a historical artifact.
