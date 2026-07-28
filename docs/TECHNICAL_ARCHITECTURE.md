# Relicbound — Technical Architecture

**Status:** foundation document. Describes the systems to be built; no gameplay
code exists yet.

The C# in this document is **illustrative sketch**, not a specification of exact
signatures. It is here to make the shape of each system concrete enough to
argue with before anyone writes it.

---

## 1. Layers

```
┌──────────────────────────────────────────────┐
│  Relicbound.Game        (game/)              │  Godot. Scenes, UI, input,
│                                              │  animation. Displays state.
└───────────────────┬──────────────────────────┘
                    │
┌───────────────────▼──────────────────────────┐
│  Relicbound.Content     (src/)               │  JSON content + loaders.
└───────────────────┬──────────────────────────┘
                    │
┌───────────────────▼──────────────────────────┐
│  Relicbound.Gameplay    (src/)               │  Combat, artifacts, spells,
│                                              │  runes, expeditions.
└───────────────────┬──────────────────────────┘
                    │
┌───────────────────▼──────────────────────────┐
│  Relicbound.Core        (src/)               │  Entities, events, effects,
│                                              │  stats, rules. No engine.
└──────────────────────────────────────────────┘
```

Dependencies flow downward only.

### Why these are separate assemblies

The single most important architectural rule is *the simulation must run without
the engine*. As folder conventions inside one project, that rule survives exactly
as long as everyone remembers it. As **separate assemblies with no Godot
reference**, `using Godot;` inside Core is a compile error.

This is also why `Core` and `Gameplay` live in `src/` rather than inside the
Godot project directory, which is a deliberate change from the original folder
sketch. Two concrete benefits:

- `dotnet test` runs the entire simulation on CI with no engine installed
- Godot's csproj globs `**/*.cs`, so nested class libraries would be compiled
  twice

`ArchitectureTests` in both test projects assert the rule on every CI run.

---

## 2. Determinism

**Same seed plus same inputs must always produce the same result.** This is
load-bearing for three reasons: runs are reproducible for bug reports, combat is
testable without mocking randomness, and a replay is just a seed plus an input
list.

### Rules

Forbidden in `Core` and `Gameplay`:

- `System.Random` without an explicit seed
- `DateTime.Now`, `Environment.TickCount`
- `Guid.NewGuid()` where the result affects the simulation
- Iterating a `Dictionary`/`HashSet` where order affects the result
- Floating-point accumulation where integers or fixed-point would serve

### Random source

```csharp
public interface IRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);
    ulong State { get; }          // serializable — a save resumes mid-stream
}
```

Systems take an `IRandomSource` as a dependency. They never construct one.

**Split streams by concern.** Map generation, loot rolls, and combat each get
their own stream derived from the run seed:

```csharp
runSeed → SplitMix64 → { mapStream, lootStream, combatStream }
```

Without this, adding one extra die roll in combat shifts every subsequent loot
roll, and two runs with the same seed diverge for no visible reason. Splitting
makes each subsystem's randomness independent and stable under change.

---

## 3. Entities and components

Composition, not inheritance. An `Entity` is an identity plus a component bag.

```csharp
public sealed class Entity
{
    public EntityId Id { get; }
    public string Name { get; }

    public T? Get<T>() where T : class, IComponent;
    public bool Has<T>() where T : class, IComponent;
    public void Add<T>(T component) where T : class, IComponent;
}
```

Components are plain data plus narrow behavior:

| Component | Holds |
| --- | --- |
| `Health` | Current, max, temporary shield |
| `StatBlock` | Base stats and active modifiers |
| `GridPosition` | Tile coordinates, facing |
| `Statuses` | Active statuses and remaining stacks |
| `Equipment` | Equipped artifacts |
| `Abilities` | Known spells and their rune configurations |
| `TurnResources` | Current AP |
| `Intent` | What this entity will do next turn (drives telegraphing) |

The player and a goblin are the same type. They differ only by components and
data. There is no `Character : Entity`, and no `Mage : Character`.

---

## 4. Stats

Stats are a base value plus ordered modifier layers. Deterministic, inspectable,
and able to explain themselves — the last part matters because the combat log has
to be able to tell the player *why* a number was what it was.

```csharp
public enum ModifierLayer
{
    Flat,       // +5 damage
    Increased,  // +30% — additive with other Increased
    More        // ×1.2 — multiplicative, applied last
}

public readonly record struct StatModifier(
    StatType Stat,
    ModifierLayer Layer,
    int Value,
    ModifierSource Source);
```

Resolution order is always Flat → Increased → More, and within a layer,
modifiers are sorted by source id. Two identical builds always compute identical
numbers.

---

## 5. Events

The event bus is how systems stay ignorant of each other. An artifact never
holds a reference to the player.

```csharp
public interface IGameEvent
{
    EventType Type { get; }
}

public interface IEventBus
{
    void Publish(IGameEvent gameEvent);
    IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent;
}
```

### The attack pipeline

From the planning notes, and the canonical example of how a turn resolves:

```
Player attacks
      │
      ▼
ATTACK_STARTED         ← modifiers can still change the attack
      │
      ▼
DAMAGE_DEALT           ← Ember Heart triggers here
      │
      ▼
ARTIFACT_TRIGGER_CHECK ← equipped artifacts matched against the event
      │
      ▼
STATUS_APPLIED         ← Burn lands; may itself trigger more artifacts
```

### Event catalogue (initial)

```
Combat lifecycle   COMBAT_STARTED, ROUND_STARTED, TURN_STARTED,
                   TURN_ENDED, ROUND_ENDED, COMBAT_ENDED
Actions            ACTION_DECLARED, ACTION_RESOLVED, AP_SPENT, AP_GAINED
Damage & health    ATTACK_STARTED, DAMAGE_CALCULATED, DAMAGE_DEALT,
                   HEALED, SHIELD_GAINED, ENTITY_DEFEATED, PLAYER_DEFEATED
Statuses           STATUS_APPLIED, STATUS_STACKED, STATUS_EXPIRED
Movement           MOVE_STARTED, MOVED
Meta               ARTIFACT_EQUIPPED, ARTIFACT_TRIGGERED,
                   SYNERGY_DISCOVERED, ITEM_ACQUIRED
```

`SYNERGY_DISCOVERED` is what writes to the Archive. Discovery is a first-class
event, not a UI afterthought.

---

## 6. Effects

Every gameplay change is an Effect. There are no ad-hoc mutation methods —
if something changes the game state, it is an Effect, so it can be triggered,
composed, logged, and tested uniformly.

```csharp
public abstract class Effect
{
    public abstract EffectResult Execute(EffectContext context);
}

public sealed class EffectContext
{
    public GameState State { get; }
    public Entity Source { get; }
    public IReadOnlyList<Entity> Targets { get; }
    public IRandomSource Random { get; }
    public int Depth { get; }          // recursion guard — see §7
    public EffectOrigin Origin { get; } // spell, artifact, status, ...
}

public sealed class EffectResult
{
    public IReadOnlyList<IGameEvent> Events { get; }
    public IReadOnlyList<Effect> Queued { get; }  // never call Execute directly
}
```

Initial set: `DamageEffect`, `HealEffect`, `ApplyStatusEffect`,
`ModifyStatEffect`, `MoveEffect`, `SummonEffect`, `GrantResourceEffect`,
`ReviveEffect`.

**An effect never executes another effect directly.** It returns effects to be
queued. That single constraint is what makes the recursion guard in §7 possible
and keeps a synergy chain from becoming a stack overflow.

---

## 7. Trigger resolution and recursion guards

*This section is not in the original planning notes. It is the part that will
break first without deliberate design.*

The moment two artifacts trigger on events the other one produces, you have a
cycle. Ember Heart applies Burn on damage; a Burn artifact deals damage on Burn;
Ember Heart triggers again. Without guards, the first genuinely interesting
synergy the player assembles hangs the game.

### Resolution loop

Iterative, never recursive:

```csharp
queue.EnqueueRange(action.Effects, depth: 0);

while (queue.TryDequeue(out var effect) && steps++ < MAX_RESOLUTION_STEPS)
{
    var result = effect.Execute(context);

    foreach (var gameEvent in result.Events)
    {
        journal.Record(gameEvent);

        foreach (var trigger in triggers.Match(gameEvent))   // deterministic order
        {
            if (effect.Depth >= MAX_TRIGGER_DEPTH) { continue; }
            if (!budget.TryConsume(trigger)) { continue; }

            queue.EnqueueRange(trigger.Effects, effect.Depth + 1);
        }
    }

    queue.EnqueueRange(result.Queued, effect.Depth);
}
```

### The four guards

1. **Depth cap** — `MAX_TRIGGER_DEPTH` (start at 8). An effect spawned by a
   trigger is one deeper than its cause. Chains terminate.
2. **Per-round trigger budget** — each artifact has a maximum firings per round
   (default 1; some artifacts explicitly allow more). Stops A→B→A ping-pong.
3. **Step ceiling** — `MAX_RESOLUTION_STEPS` (start at 512) as a backstop. Hitting
   it is a content bug: log loudly, fail the test suite, do not fail silently.
4. **Deterministic trigger order** — matched triggers are sorted by
   (equipment slot index, then artifact id). Never dictionary order. Two players
   with the same build see the same chain in the same sequence.

> These are tunable numbers, not laws. But they must exist from the first
> artifact, because retrofitting them after a content library exists means
> re-balancing everything.

---

## 8. Turn and action-point model

Per `GAME_DESIGN.md`, the player has **3 AP per turn** and actions declare costs.
The economy sits behind an abstraction so it can be changed without touching the
systems built on it.

```csharp
public readonly record struct ActionCost(int ActionPoints);

public interface ITurnResourceModel
{
    int StartingResources(Entity entity);
    bool CanAfford(Entity entity, ActionCost cost);
    void Spend(Entity entity, ActionCost cost);
    void Refund(Entity entity, int amount);
}
```

- **`ActionPointModel`** — the default. 3 AP, refreshed each turn, modifiable by
  artifacts via `GrantResourceEffect` and cost modifiers.
- **`BinaryActionModel`** — move-plus-one-action, kept as the documented
  fallback. Implementing it is a config change, not a rewrite.

Artifacts hook the economy at four points, which is the whole reason AP was
chosen: **income** (AP per turn), **cost** (spell discounts), **refund** (AP back
on a condition), and **conversion** (health → AP, for the Blood and Sacrifice
tags).

---

## 9. Combat flow

```
CombatSimulation.Run(CombatSetup) → CombatResult
```

The simulation is a plain object with no engine dependency. It can be run in a
test, in CI, or a thousand times in a loop for balance analysis.

```
COMBAT_STARTED
└─ loop:
   ROUND_STARTED
   ├─ tick statuses (Burn damage, Chill decrement, ...)
   ├─ fire ROUND_START triggers
   ├─ enemies declare intent          ← telegraphing
   ├─ TURN_STARTED (player)
   │  └─ until AP exhausted or player ends turn:
   │     ACTION_DECLARED → validate cost → resolve effects (§7) → ACTION_RESOLVED
   ├─ TURN_ENDED (player)
   ├─ TURN_STARTED (enemies) → execute declared intents
   ├─ TURN_ENDED (enemies)
   └─ ROUND_ENDED  → check victory / defeat
COMBAT_ENDED
```

### The Journal

Every event is recorded to a **Journal** in resolution order. This is one
structure serving three purposes, which is why it is worth calling out:

1. The **combat log** the player reads — the game's primary teaching surface
2. The **animation feed** Godot plays back
3. The **assertion target** in tests: *given this build, this event sequence*

Because presentation is a replay of the journal, the engine cannot desync from
the simulation — there is only one source of truth, and Godot is downstream of
it.

---

## 10. Content pipeline

Content is JSON in `src/Relicbound.Content/`, embedded as resources so it loads
identically in a test, on CI, and in the built game.

```
Artifacts/  Spells/  Runes/  Enemies/  Encounters/  Schemas/
```

**Load once at startup, validate everything, fail loudly.** A content error is a
build error, not a mystery at runtime:

- every `id` unique
- every referenced trigger, effect, status, and tag exists
- numeric fields within declared ranges
- report **all** failures at once, not just the first

```csharp
public interface IContentRepository
{
    ArtifactDefinition Artifact(string id);
    SpellDefinition Spell(string id);
    RuneDefinition Rune(string id);
    IReadOnlyList<ArtifactDefinition> ArtifactsWithTag(Tag tag);
}
```

Adding an artifact means adding a JSON file. If it requires a code change, the
system is wrong — fix the system, per `CODING_STANDARDS.md`.

---

## 11. Presentation binding

Godot's job is narrow: **turn input into requests, and turn journal entries into
animation.**

```
Player clicks a tile
      │
      ▼
CombatView  ──►  ActionRequest(Move, target)  ──►  CombatSimulation
                                                        │
                                          resolves, appends to Journal
                                                        │
CombatView  ◄──  journal entries  ◄─────────────────────┘
      │
      ▼
Play animations, update the log
```

Rules:

- Scenes hold **no** game state. They render what the simulation says.
- UI **requests**; it never decides. A button says "request attack," not
  "apply damage."
- View scripts stay small: `PlayerView.cs` handles input and animation, nothing
  else.
- Animation is a *presentation* of the journal. If animation lags, the
  simulation does not wait — it has already resolved.

---

## 12. Save format

Versioned JSON. Two separate concerns:

```
Profile   (permanent)  Archive entries, Vault contents, unlock state, settings
Run       (in-flight)  run seed, RNG stream states, expedition map, player state
```

Every save carries a schema version and a migration path. The run save stores the
**seed and stream states**, not a full state dump, which is only possible because
the simulation is deterministic (§2) — the state can be rebuilt by replaying.

---

## 13. Testing strategy

Tests run headlessly. If a test needs Godot, the logic is in the wrong layer.

| Priority | What | Where |
| --- | --- | --- |
| 1 | Damage calculation, stat resolution | `Core.Tests` |
| 2 | Effect execution and composition | `Core.Tests` |
| 3 | Artifact triggers, including chains and guards | `Gameplay.Tests` |
| 4 | Rune composition (cost, tags, naming) | `Gameplay.Tests` |
| 5 | Status interactions | `Gameplay.Tests` |
| 6 | Architecture rules | both |

Given/When/Then, named so a failure reads as a sentence:

```csharp
[Fact]
public void Player_WithPhoenixFeather_RevivesAtZeroHealth()

[Fact]
public void EmberHeart_AppliesBurn_WhenDamageIsDealt()

[Fact]
public void TriggerChain_TerminatesAtMaxDepth_WhenArtifactsTriggerEachOther()
```

That last one is not optional. The cycle described in §7 should have a test
before the artifact that causes it exists.

### Balance harness

Because combat is a plain object, a test can run ten thousand simulated fights
with a given build and report win rate and average rounds. This is the cheapest
balance tool available and it costs nothing extra — it falls straight out of the
headless architecture.

---

## 14. Build and tooling

| | |
| --- | --- |
| Engine | Godot 4.7.1 (.NET / C# build) |
| SDK | `Godot.NET.Sdk/4.7.0` |
| Target framework | `net8.0` |
| Renderer | GL Compatibility |
| Resolution | 1280×720, `canvas_items` stretch |
| Tests | xUnit |
| CI | GitHub Actions — `dotnet build` + `dotnet test`, no engine required |

```bash
dotnet test Relicbound.sln     # entire simulation, no Godot needed
```

---

## 15. Decisions deliberately deferred

Not needed to prove the prototype's hypothesis; listed so nobody re-litigates
them mid-milestone:

- Networking of any kind — permanently out of scope
- ECS with archetype storage — the component bag in §3 is enough at this scale
- Custom serialization — `System.Text.Json` until it measurably hurts
- Object pooling / allocation tuning — turn-based, tiny board; not a bottleneck
- Localization — after the prototype proves the loop
- Mod support — the data-driven content pipeline makes it possible later; do not
  design for it now
