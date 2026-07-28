# Relicbound

A 2D top-down tactical roguelite RPG about **discovering relics and the builds
they make possible**.

You descend into ruins, recover artifacts, and find out what they do when you put
them next to each other. Most of the time you don't come back with the artifact.
You always come back with what you learned.

> **Status: Milestone 1 in progress.** Entity, Event, and Effect systems are
> implemented and tested in `Relicbound.Core`. Grid movement, the AP turn
> model, the combat simulation loop, and the Godot view are next. See
> [`docs/PROTOTYPE_ROADMAP.md`](docs/PROTOTYPE_ROADMAP.md).

---

## The idea in one paragraph

Combat is a turn-based tactical grid with 3 action points per turn — but combat
isn't the point. It's the stage that builds perform on. Artifacts, spells, and
runes all interact through a shared **tag** vocabulary (Fire, Blood, Spirit,
Time…), so a synergy written once works with every piece of content that carries
those tags, including content added months later. Progression is mostly
*knowledge*: what survives a failed run is a permanent Archive of what you
discovered.

---

## Requirements

| | |
| --- | --- |
| Engine | [Godot 4.7.1](https://godotengine.org/download) — **.NET / C# build** |
| SDK | .NET 8 SDK |

The plain Godot build won't work — you need the .NET one.

## Getting started

```bash
git clone https://github.com/steven-robert-eddy/Relicbound.git
cd Relicbound

# Run the simulation tests. No Godot required.
dotnet test
```

To open the game: launch Godot 4.7.1 (.NET) and import `game/project.godot`.

Godot will generate its `.godot/` cache on first open — that's ignored by git.
The project runs to an empty 1280×720 window; `game/Scenes/Main.tscn` is an empty
bootstrap scene that Milestone 1 replaces.

### Using a different Godot version

Change the SDK version in `game/Relicbound.Game.csproj`:

```xml
<Project Sdk="Godot.NET.Sdk/4.7.0">
```

and the feature tag in `game/project.godot`:

```ini
config/features=PackedStringArray("4.7", "C#", "GL Compatibility")
```

---

## Repository layout

```
docs/       Design and architecture. Read before writing code.
src/        Pure C# simulation — no engine dependency. The game lives here.
  Relicbound.Core/       Entities, events, effects, stats, rules
  Relicbound.Gameplay/   Combat, artifacts, spells, runes, expeditions
  Relicbound.Content/    JSON content and loaders
game/       Godot project. Presentation only.
tests/      xUnit tests. Run headlessly.
```

### Why the simulation is separate from the engine

`Relicbound.Core` and `Relicbound.Gameplay` are class libraries with **no Godot
reference**, so `using Godot;` inside them is a compile error rather than a code
review comment. Combat runs headlessly, which means:

- CI tests the entire game logic without installing an engine
- A build can be simulated ten thousand times for balance analysis
- Godot is downstream of the simulation and can't desync from it

`ArchitectureTests` in both test projects enforce this on every CI run.

---

## Documentation

| Document | What it covers |
| --- | --- |
| [`AGENTS.md`](AGENTS.md) | Contract for AI agents working in this repo |
| [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) | Premise, pillars, combat, artifacts, meta-progression |
| [`docs/TECHNICAL_ARCHITECTURE.md`](docs/TECHNICAL_ARCHITECTURE.md) | Layers, events, effects, determinism, trigger guards |
| [`docs/CODING_STANDARDS.md`](docs/CODING_STANDARDS.md) | Naming, structure, dependency and testing rules |
| [`docs/PROTOTYPE_ROADMAP.md`](docs/PROTOTYPE_ROADMAP.md) | Four milestones with acceptance criteria |
| [`docs/MILESTONES.md`](docs/MILESTONES.md) | Issue-level breakdown of each milestone |

**`GAME_DESIGN.md` contains marked `ASSUMPTION` blocks** — design decisions
drafted to fill gaps, awaiting review. Start there.

---

## Design inspirations

**Hades** for presentation · **Into the Breach** for combat clarity ·
**Gloomhaven** for tactical feel · **Vault Hunters** for loot and build discovery

---

## What this prototype is for

One question:

> Is exploring, discovering artifacts, and creating builds fun?

Not whether it's polished, balanced, or big. Everything outside that question is
deliberately out of scope — see the scope protection list in
[`AGENTS.md`](AGENTS.md).
