# Relicbound — Game Design

**Status:** first draft. Contains decisions that need your review.

## How to read this document

Relicbound's design came out of a planning conversation that settled the
technology and the shape of the game but did not write down the game itself.
This document fills that gap. To make review fast, every statement is marked:

| Marker | Meaning |
| --- | --- |
| *(unmarked)* | Established — came from your planning notes or from decisions you made directly |
| > **ASSUMPTION** | Invented to fill a gap. Plausible, but you have not decided it. **Read these first.** |
| > **OPEN** | A real question with no good default. Needs a decision before it matters. |

Correcting the ASSUMPTION blocks is the intended next step. Nothing downstream
depends on them being right — they exist so the document is concrete enough to
argue with.

---

## 1. Premise

> **ASSUMPTION** — the entire fiction below is invented. Only the title and the
> words "relic", "expedition", "workshop", and "archive" came from your notes.

An age ago, a civilization learned to bind power into objects. It did not end
well for them. What remains are the relics, scattered through ruins that have
grown strange in the centuries since.

You are one of the **Bound** — someone who can attune to a relic and survive the
experience. You descend, you recover what you can, and most of the time you do
not come back with it. What you *do* come back with is knowledge: what that relic
was, what it does, and what happens when it sits next to another one.

The fiction exists to justify three mechanics, and no more than that:

- **Relics are found, not bought.** Discovery drives the loop.
- **Attunement is dangerous.** Carrying power draws attention — this is the
  in-world reason difficulty scales with what you bring.
- **Knowledge outlives the run.** You lose the relic; you keep what you learned.

---

## 2. Design pillars

Every design decision should be traceable to one of these. If it isn't, cut it.

### Discovery over power

The best moment in Relicbound is not "my numbers got bigger." It is *"wait — does
that stack?"* Content should be designed to be found out, not read off a tooltip.

### Builds emerge, they are not chosen

There are no classes. There is no skill tree. Your identity in a run is entirely
the set of artifacts, spells, and runes you happened to find and chose to keep.
Two players with the same starting conditions should be able to end up playing
very different games.

### Legible tactics

Combat is the stage builds perform on. If the player cannot tell *why* something
happened, the build system is invisible and the game fails. Full information,
readable enemy intent, no hidden rolls where it matters.

### Runs matter

A run has to be losable, and losing has to cost something real, or discovery has
no weight. What survives a loss is knowledge — not power.

---

## 3. The core loop

```
        WORKSHOP  ──────────────────┐
   (choose loadout, review Archive) │
            │                       │
            ▼                       │
        EXPEDITION                  │
   (node map: combat, elite,        │
    treasure, event, merchant)      │
            │                       │
            ▼                       │
         COMBAT                     │
  (grid, turn-based, 3 AP)          │
            │                       │
            ▼                       │
      BOSS ── win ─► RETURN ────────┤
            │                       │
           lose                     │
            │                       │
            ▼                       │
      DEATH ── relics lost ─────────┘
            │
            ▼
      ARCHIVE GROWS
   (everything you learned is permanent)
```

The loop's engine is the Archive. Every run, win or lose, makes the *player*
better informed — which is a form of progression that never breaks the game's
math.

---

## 4. Combat

### Model

Square grid, turn-based, full information. Enemies telegraph their intent before
the player commits, in the style of Into the Breach.

> **ASSUMPTION** — enemy intent telegraphing. Your notes cite Into the Breach for
> "combat clarity," which I read as intent visibility. If you meant only visual
> readability, say so; it's a meaningful difference.

### Action points

**The player has 3 AP per turn.** Actions declare a cost.

| Action | Typical cost |
| --- | --- |
| Move one tile | 1 |
| Basic attack (Strike) | 1 |
| Light spell (Bolt) | 1 |
| Standard spell (Fire Bolt, Shield) | 2 |
| Heavy spell | 3 |

**Why 3 and not 2 or 4.** Two AP is move-plus-action wearing a costume — there is
no decision in it. Three is the smallest number that forces a real trade-off
every single turn: reposition twice and swing, hold ground and cast twice, or
move once and spend two on something big. Four starts to allow full combos every
turn, which flattens the tension and inflates enemy health to compensate.

AP is also the substrate the build system needs. It gives artifacts something to
manipulate that isn't a damage number:

- **Income** — "gain 1 AP on the turn after you kill something"
- **Cost** — "Fire spells cost 1 less, minimum 1"
- **Refund** — "when a Burn expires, refund 1 AP"
- **Conversion** — "spend 5 health to gain 2 AP" (this is what the **Blood** and
  **Sacrifice** tags are for)

This is the concrete reason AP was chosen over move-plus-action: the tag list you
already wrote down — Blood, Sacrifice, Time — only means something mechanically
if there is a resource to trade against.

> **OPEN** — starting AP is a tuning value, not a law. The architecture keeps the
> economy behind a swappable abstraction (see `TECHNICAL_ARCHITECTURE.md`), so
> changing to 2 or 4, or to move-plus-action entirely, is a config change.

### Turn structure

```
ROUND START
  ├─ Statuses tick (Burn deals damage, Chill decrements, ...)
  ├─ Artifacts with ROUND_START triggers fire
  ├─ PLAYER TURN — spend up to 3 AP, in any order, then end turn
  └─ ENEMY TURN — enemies execute the intents they telegraphed
ROUND END
```

### What the player sees

Taken directly from your planning notes:

```
+--------------------+
|                    |
|        🔥          |
|      Goblin        |
|                    |
|            🧙      |
|                    |
+--------------------+
| Fire Bolt          |
| Shield             |
| Strike             |
|                    |
| Artifact:          |
| Ember Heart        |
+--------------------+
| Combat Log:        |
| Burn applied       |
+--------------------+
```

The combat log is not a debug tool — it is the primary teaching surface. It is
how the player discovers that their artifact did something. Treat it as a
first-class feature.

---

## 5. Tags — the interaction substrate

```
Fire   Ice    Blood   Spirit   Void      Nature
Time   Summon Sacrifice   Defense   Critical
```

Tags are how content interacts without content knowing about other content. An
artifact reads "when a **Fire** effect resolves" — never "when Ember Heart
triggers." Every artifact that ever carries the Fire tag then works with it, for
free, including ones written months later.

This is the single most important rule in the design. A synergy keyed to two
specific items is *content*. A synergy keyed to two tags is a *system*.

> **ASSUMPTION** — suggested tag identities, so content authors have a shared
> vocabulary. All invented:
>
> | Tag | Fantasy | Mechanical hook |
> | --- | --- | --- |
> | **Fire** | Damage over time, escalation | Burn stacks; rewards sustained pressure |
> | **Ice** | Control, denial | Chill/Freeze; reduces enemy AP |
> | **Blood** | Pay health for power | Health-as-resource conversions |
> | **Spirit** | Persistence, echoes | Effects that repeat or linger past death |
> | **Void** | Removal, negation | Deletes buffs, statuses, or actions |
> | **Nature** | Growth, sustain | Healing, regeneration, scaling over time |
> | **Time** | Extra actions, rewinds | AP income; acting out of turn |
> | **Summon** | Bodies on the board | Allied entities |
> | **Sacrifice** | Trade something away | Consume a resource or an ally for effect |
> | **Defense** | Mitigation, positioning | Shields, armor, forced movement |
> | **Critical** | Spikes, variance | Conditional multipliers |

Known synergies are discovered, not documented in-game up front. The Workshop
shows what you have found and leaves the rest as `???`.

---

## 6. Artifacts

An artifact is data: **tags + trigger + effects**. No artifact has bespoke code.

The player carries **3 artifacts** at a time.

### Ember Heart *(canonical — from your notes)*

```json
{
  "id": "ember_heart",
  "name": "Ember Heart",
  "tags": ["Fire", "Spirit"],
  "trigger": "DAMAGE_DEALT",
  "effects": ["APPLY_BURN"]
}
```

### Phoenix Feather *(canonical — from your notes)*

```json
{
  "id": "phoenix_feather",
  "name": "Phoenix Feather",
  "tags": ["Fire", "Spirit"],
  "trigger": "PLAYER_DEFEATED",
  "effects": ["REVIVE"]
}
```

These two share **Fire + Spirit** — which is exactly the synergy pair your
Workshop mockup showed as discovered. That is the design working: two
independently-authored artifacts produce a named synergy because their tags
overlap, not because anyone wrote "if Ember Heart and Phoenix Feather."

### Writing a good artifact

Per `CODING_STANDARDS.md`, every artifact must answer *what strategy does this
enable?*

```
Bad:   "Increase damage by 5%."
Good:  "Fire damage increases every turn until you take damage."
```

The first is a number. The second is a decision — it makes the player consider
whether to keep pressing or play safe, and it creates obvious tension with any
Blood artifact that wants them to spend health.

---

## 7. Spells and runes

A spell is a **base spell** plus **runes** in sockets. Runes modify behavior and
contribute tags, cost, and a name fragment.

The canonical example from your notes:

```
Bolt  +  Flame Rune  +  Chain Rune  =  Inferno Chain Bolt
```

> **ASSUMPTION** — the composition mechanics below are invented; only the example
> above is yours.

```json
{
  "id": "bolt",
  "name": "Bolt",
  "tags": ["Void"],
  "cost": 1,
  "range": 4,
  "sockets": 2,
  "effects": [{ "type": "DAMAGE", "amount": 6 }]
}
```

```json
{
  "id": "rune_flame",
  "name": "Flame Rune",
  "namePrefix": "Inferno",
  "addsTags": ["Fire"],
  "costDelta": 0,
  "effects": [{ "type": "APPLY_STATUS", "status": "BURN", "stacks": 2 }]
}
```

```json
{
  "id": "rune_chain",
  "name": "Chain Rune",
  "namePrefix": "Chain",
  "addsTags": [],
  "costDelta": 1,
  "modifiers": [{ "type": "CHAIN", "additionalTargets": 2, "falloff": 0.5 }]
}
```

**Composed name** is deterministic: rune prefixes in socket order, then the base
name. `Inferno` + `Chain` + `Bolt` → **Inferno Chain Bolt**, matching your
example exactly.

**Composed cost** is base cost plus the sum of `costDelta`, floored at 1. So
Inferno Chain Bolt costs 2 AP — two thirds of a turn, which is a real decision.

**Composed tags** are the union of base and rune tags. This matters more than it
looks: socketing a Flame Rune makes that spell count as **Fire** for every
Fire-tagged artifact the player is carrying. Runes are not just spell upgrades —
they are how the player *steers a build toward a tag*.

---

## 8. Expeditions

A node map, in the shape from your notes:

```
              Boss
               │
      Treasure ─┴─ Elite
               │
        Start ─┴─ Event
               │
            Merchant
```

> **ASSUMPTION** — node types and expedition length below are invented.

| Node | Purpose |
| --- | --- |
| **Combat** | The default. Teaches enemy patterns. |
| **Elite** | Harder fight, guaranteed artifact. |
| **Treasure** | Artifact or rune, no fight. |
| **Event** | A choice, often a trade. Where Blood/Sacrifice tags get interesting. |
| **Merchant** | Spend recovered materials on runes, rerolls, socket expansion. |
| **Boss** | Ends the expedition. Survive it and you get to choose what to Vault. |

> **OPEN** — expedition length. I would start at **3 floors, ~12 nodes, 20–30
> minutes**, short enough that losing does not sting and you immediately want
> another. This needs playtesting more than it needs a decision now.

---

## 9. Meta-progression

This section answers your requirement directly: *knowledge, plus some kind of
stash, but runs need to keep demanding adaptation and difficulty has to scale so
later runs don't get too easy.*

Three systems, each doing one job.

### 9.1 The Archive — permanent knowledge

Everything you *learn* is kept forever:

- Artifacts you have identified, with full text
- Tag synergies you have triggered — the `Fire + Spirit` / `???` list from your
  Workshop mockup
- Enemy attack patterns and intents
- Rune combinations you have assembled

**The Archive never grants power.** It grants information. This is what makes it
safe as an unbounded progression track: a player 100 hours in knows vastly more
than a new player but is not numerically stronger, so the combat math never has
to be rebalanced around it. It is also the pillar — *discovery over power* — made
into a system.

### 9.2 The Vault — a small, deliberate stash

Survive a boss and you may deposit **artifacts into the Vault**, starting with
**3 slots**. On a later expedition you may carry Vaulted artifacts in.

> **ASSUMPTION** — 3 slots, deposit-on-boss-clear only, and the rule below.

**The Vault gives you a seed, not a strategy.** Carried artifacts fill your
loadout, but the artifacts *offered during the run* are drawn from a seeded pool
you cannot control. So the Vault can start you toward Fire — it can never hand
you a finished Fire build. Adaptation stays mandatory, which is what you asked
for.

### 9.3 Resonance — the scaler

**This is the mechanism that stops the stash from trivializing the game.**

Every artifact carried in from the Vault adds **Resonance**. Higher Resonance
escalates the expedition:

| Resonance | Effect |
| --- | --- |
| 0 | Baseline expedition |
| 1–2 | Enemy tiers raised; elites gain an affix |
| 3–4 | Additional elite nodes; boss gains a second phase |
| 5+ | Enemy affixes stack; boss variants with new mechanics |

And it raises the rewards to match: better artifact rarity, more rune drops,
larger Vault deposit allowance.

**Why this works rather than merely patching the problem.** Bringing power in is
a *wager*, not a head start. Three strong Vaulted artifacts mean a genuinely
harder run, so the stash cannot make the game easier — it makes it *bigger* in
both directions. The player decides how much game they want today. Difficulty
scaling stops being a balance chore and becomes a player-facing choice.

It also happens to be exactly on theme for a game called Relicbound: what you
carry binds you, and the deep places notice what you're carrying.

> **ASSUMPTION** — Resonance is entirely invented, and it is the single biggest
> design decision in this document. The documented alternative is an explicit
> opt-in difficulty ladder (Hades' Heat, Slay the Spire's Ascension): the player
> ticks modifiers on for better rewards. That is proven and simpler, but it is a
> menu rather than a mechanic, and it does not solve the stash problem — you
> would still need something to stop Vaulted power from compounding. The two can
> coexist: Resonance for in-fiction scaling, a Descent ladder for mastery.

### 9.4 What is lost on death

Everything carried: artifacts, runes, spell configuration, materials.
Kept: the Archive, and the Vault as it stood when you left.

> **OPEN** — should death cost you the Vaulted artifacts you *brought with you*?
> Yes is the tense, coherent answer — it makes carrying power genuinely risky and
> keeps Resonance honest. No is the friendlier answer. I lean yes, with the Vault
> slot itself preserved so you can refill it.

---

## 10. The Workshop

The between-runs hub, from your notes:

```
Artifact Forge

Current Build:
  [Ember Heart]  [Phoenix Feather]  [Empty]

Known Synergies:
  Fire + Spirit
  ???

  [ Experiment ]
```

Three jobs:

1. **Loadout** — choose which Vaulted artifacts to carry, and see the Resonance
   they will cost you.
2. **Archive** — browse what you have learned. The `???` entries are the hook;
   they should be visible and countable, so the player knows how much is left to
   find.
3. **Experiment** — simulate a combat encounter against a known enemy with your
   current loadout, with no stakes.

> **ASSUMPTION** — "Experiment" is invented from the button in your mockup. It is
> an unusually good fit for this architecture: because the simulation runs
> headlessly with no engine dependency, a no-stakes sandbox is nearly free to
> build. It also directly serves *discovery over power* — it is a place to find
> out what stacks without dying to find out.

---

## 11. Difficulty and run variance

Three independent sources of pressure, so that no single one has to carry the
game:

1. **Depth** — enemy tier rises per floor within an expedition.
2. **Resonance** — rises with what you carried in (§9.3).
3. **Descent** *(post-prototype)* — an optional mastery ladder for players who
   have exhausted the first two.

And three sources of variance, so runs demand adaptation:

- Seeded expedition layout — the node map differs every run
- Seeded artifact pool — you cannot count on assembling the same build twice
- Boss variants — the same boss with a different second phase

---

## 12. Out of scope for the prototype

From your notes, restated because it is easy to forget mid-build:

Multiplayer · online features · large open world · complex dialogue ·
procedural AI content generation · economy systems · advanced crafting

The prototype exists to prove one thing:

> Exploring, discovering artifacts, and creating builds is fun.

---

## 13. Open questions

Collected from the ASSUMPTION and OPEN markers above, in the order they will
start to matter:

| # | Question | Needed by |
| --- | --- | --- |
| 1 | Is the Bound / fallen-civilization premise right? | Whenever content gets names |
| 2 | Do enemies telegraph intent explicitly? | Milestone 1 |
| 3 | Are the tag identities in §5 right? | Milestone 2 |
| 4 | Resonance, or an opt-in difficulty ladder, or both? | Milestone 4 |
| 5 | Does death cost you the artifacts you carried in? | Milestone 4 |
| 6 | Expedition length — 3 floors? | Milestone 4 |
| 7 | Is 3 AP right? | Playtest, not now |
