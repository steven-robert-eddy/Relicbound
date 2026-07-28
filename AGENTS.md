# AGENTS.md — Working on Relicbound

Instructions for AI agents contributing to this repository. Read this before
writing any code.

---

## What Relicbound is

A 2D top-down tactical roguelite RPG about **discovering relics and the builds
they make possible**. The player descends into ruins, recovers artifacts, learns
what those artifacts do in combination, and carries that knowledge into the next
run.

The prototype exists to prove exactly one hypothesis:

> Exploring, discovering artifacts, and creating builds is fun.

Everything that does not serve that hypothesis is out of scope.

---

## Required reading order

1. `AGENTS.md` (this file)
2. `docs/GAME_DESIGN.md` — what the game is
3. `docs/TECHNICAL_ARCHITECTURE.md` — how the systems fit together
4. `docs/CODING_STANDARDS.md` — how to write the code
5. `docs/PROTOTYPE_ROADMAP.md` / `docs/MILESTONES.md` — what to build next

---

## Rules that must never be broken

These are not style preferences. Breaking one is a bug.

1. **`Relicbound.Core` and `Relicbound.Gameplay` never reference Godot.**
   The simulation runs headlessly. This is enforced by assembly boundaries and
   by `ArchitectureTests` in both test projects.

2. **Dependencies flow downward only:** `Game → Content → Gameplay → Core`.
   Nothing reaches back up.

3. **Content is data, not code.** A new artifact, spell, rune, or enemy should
   be a JSON file. If adding content requires a `switch` on a name, the system
   is wrong — fix the system.

4. **Gameplay changes are Effects.** Damage, healing, statuses, stat changes,
   summons, movement — all Effects. Not ad-hoc methods.

5. **Systems talk through events**, not direct references. An artifact never
   holds a pointer to the player.

6. **Simulation is deterministic.** Same seed plus same inputs equals same
   result, always. All randomness comes from an injected seeded source. No
   `DateTime.Now`, no unseeded `Random`, no order-dependent dictionary
   iteration.

7. **Scenes display; they do not decide.** UI requests actions. The simulation
   resolves them.

---

## Scope protection

Do not add, even if it seems easy or obviously useful:

- Multiplayer or any online feature
- Large open world
- Complex dialogue systems
- Procedural AI content generation
- Economy systems
- Advanced crafting
- Art or audio polish ahead of systems

If a task seems to require one of these, stop and ask.

---

## How to work

**Before coding:**

- Read the docs above. Do not infer architecture from surrounding code alone.
- Search for an existing system that already does what you need. This codebase
  is deliberately small; reuse is usually possible.
- State your design decisions and why, before writing the implementation.
- If the request is ambiguous in a way that changes the design, ask rather than
  guess.

**While coding:**

- Match the surrounding style. `.editorconfig` and `docs/CODING_STANDARDS.md`
  are authoritative.
- Prefer composition over inheritance. Deep hierarchies are a smell here.
- Keep classes to one responsibility.
- Write the test alongside the system, not afterward.

**After coding:**

- Run `dotnet test`. It must pass without Godot installed.
- Update the docs if you changed architecture.
- Say plainly what you did, what you did not do, and anything you left broken.

---

## Definition of done

A change is done when all of these are true:

- [ ] `dotnet build` succeeds with no new warnings
- [ ] `dotnet test` passes
- [ ] New gameplay systems have tests covering their rules
- [ ] No new Godot dependency in `Core` or `Gameplay`
- [ ] New content is data, not code
- [ ] Docs updated if architecture changed
- [ ] Scope unchanged from what was asked

---

## Repository layout

```
docs/       Design and architecture documentation — read before coding
src/        Pure C# simulation. No engine. This is where the game actually lives.
game/       Godot project. Presentation only.
tests/      xUnit tests. Run headlessly.
```

---

## Starting Milestone 1

When picking up the first implementation task, the scope is deliberately narrow:

> Read `AGENTS.md`, `docs/GAME_DESIGN.md`, `docs/TECHNICAL_ARCHITECTURE.md`, and
> `docs/MILESTONES.md`. We are starting **Milestone 1: The Spark**. Create the
> project foundation and implement only the Entity, Event, and Effect systems.
> Do not add combat UI or content yet. Explain architecture decisions before
> coding.

Resist the urge to build ahead. The milestones are ordered so that each one is
provable on its own.
