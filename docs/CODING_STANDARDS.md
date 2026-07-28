# Relicbound Coding Standards

## Purpose

This document defines coding standards and architectural rules for Relicbound.

The goal is to maintain a clean, expandable codebase that supports:

- Data-driven gameplay
- Modular systems
- Easy testing
- AI-assisted development
- Long-term expansion

When uncertain, prefer simplicity and maintainability over clever solutions.

---

# General Principles

## 1. Build Systems, Not Features

Before implementing a feature, ask:

> "Could another feature use this same system?"

**Bad:** Creating special code for the Phoenix Feather artifact.

**Good:** Creating a reusable trigger and effect system that Phoenix Feather uses.

## 2. Avoid Hardcoded Gameplay

Gameplay content should not live inside core systems.

Avoid:

```csharp
if (artifactName == "Phoenix Feather")
{
    RevivePlayer();
}
```

Prefer:

```json
{
  "trigger": "PLAYER_DEFEATED",
  "effect": "REVIVE"
}
```

## 3. Prefer Composition Over Inheritance

Avoid deep inheritance trees.

Bad:

```
Entity
 └── Character
      └── Mage
           └── FireMage
                └── PhoenixFireMage
```

Prefer:

```
Entity

Components:
- Health
- Stats
- Effects
- Equipment
- Abilities
```

---

# Project Structure Rules

## Core

Contains pure game logic.

```
src/Relicbound.Core/
    Events/
    Effects/
    Entities/
    Stats/
    Rules/
```

Core must not depend on:

- Godot scenes
- Sprites
- UI
- Animations

**This is enforced by the compiler, not by discipline.** `Relicbound.Core` is a
separate assembly with no Godot reference, so `using Godot;` inside Core is a
build error. `tests/Relicbound.Core.Tests/ArchitectureTests.cs` re-checks it on
every CI run.

## Gameplay

Contains game-specific systems.

```
src/Relicbound.Gameplay/
    Combat/
    Artifacts/
    Spells/
    Runes/
    Expeditions/
```

Gameplay may depend on Core. It may not depend on Godot.

## Content

Contains data files.

```
src/Relicbound.Content/
    Artifacts/
    Spells/
    Runes/
    Enemies/
    Encounters/
```

Adding new content should rarely require code changes.

## Scenes

Contains Godot presentation.

```
game/Scenes/
    Player/
    Combat/
    Dungeon/
    Workshop/
```

Scenes display game state. They do not contain gameplay rules.

---

# C# Standards

## Naming

| Kind | Convention | Example |
| --- | --- | --- |
| Classes | PascalCase | `ArtifactSystem`, `CombatManager`, `FireBoltEffect` |
| Methods | PascalCase | `ApplyDamage()`, `CalculateStats()`, `TriggerEffect()` |
| Properties | PascalCase | `CurrentHealth`, `ActionPoints` |
| Local variables | camelCase | `currentHealth`, `artifactList`, `targetEntity` |
| Parameters | camelCase | `targetEntity`, `damageAmount` |
| Private fields | `_camelCase` | `_eventBus`, `_activeStatuses` |
| Interfaces | `IPascalCase` | `IEffect`, `IRandomSource` |
| Constants | UPPER_CASE | `MAX_HEALTH`, `DEFAULT_DAMAGE` |

> **Note on constants:** `UPPER_CASE` is the house style for this project. It is
> deliberately not the idiomatic C# convention (which is PascalCase). `.editorconfig`
> is configured to match, so analyzers will not fight it. If you ever prefer the
> idiomatic form, change both this table and `.editorconfig` together.

## Class Responsibilities

A class should have one clear responsibility.

Bad:

```
GameManager
- Handles combat
- Saves files
- Updates UI
- Loads enemies
- Creates artifacts
- Controls player movement
```

Good:

```
CombatManager
SaveManager
ArtifactLoader
PlayerController
```

## Dependency Rules

Dependencies flow downward:

```
Game (Godot)
   |
Content
   |
Gameplay
   |
Core
```

Never the other way. Core systems must be usable without graphics.

## Events

Systems communicate through events.

```
Player attacks
      |
      v
ATTACK_STARTED
      |
      v
DAMAGE_DEALT
      |
      v
ARTIFACT_TRIGGER_CHECK
      |
      v
STATUS_APPLIED
```

Avoid direct references:

```csharp
// Bad
phoenixFeather.Revive(player);

// Better
eventBus.Publish(new EntityDefeatedEvent(player));
```

## Effects

Effects are the foundation of gameplay. All major gameplay changes should be
represented as effects.

Examples: `DamageEffect`, `HealEffect`, `StatusEffect`, `ModifyStatEffect`,
`SummonEffect`, `MoveEffect`.

Effects must be reusable, testable, and composable.

## Determinism

**Core and Gameplay must be deterministic.** The same seed and the same inputs
must always produce the same result. This is not academic: it is what makes runs
reproducible, bugs reportable, and combat testable.

Forbidden in Core and Gameplay:

- `System.Random` constructed without an explicit seed
- `DateTime.Now`, `DateTime.UtcNow`, `Environment.TickCount`
- `Guid.NewGuid()` for anything that affects simulation outcome
- Iterating a `Dictionary` or `HashSet` where iteration order affects the result
- Floating-point accumulation where integer or fixed-point math would do

All randomness flows through an injected seeded random source. If a system needs
randomness, it takes it as a dependency; it never creates its own.

---

# Data Standards

## Content Should Be External

Artifacts, spells, and enemies use data files.

```json
{
  "id": "ember_heart",
  "name": "Ember Heart",
  "tags": ["Fire", "Spirit"],
  "trigger": "DAMAGE_DEALT",
  "effects": ["APPLY_BURN"]
}
```

## Tags

Use tags to enable interactions.

```
Fire   Ice    Blood   Spirit   Void      Nature
Time   Summon Sacrifice   Defense   Critical
```

Avoid special-case interactions. Prefer:

```
Fire + Spirit
```

over:

```
Phoenix Feather + Ember Heart
```

A synergy keyed to two specific items is content. A synergy keyed to two tags is
a system — and every future artifact carrying those tags inherits it for free.

---

# Testing Standards

Important systems require tests. Prioritize:

- Damage calculations
- Effects
- Artifact triggers
- Rune interactions
- Status effects

Write tests in Given/When/Then form, and name them so a failure reads as a
sentence:

```csharp
[Fact]
public void Player_WithPhoenixFeather_RevivesAtZeroHealth()
```

```
Given: Player has Phoenix Feather
When:  Player reaches zero health
Then:  Player revives
```

Tests must never require Godot. If a test needs the engine, the logic is in the
wrong layer.

---

# Godot Standards

## Scenes Are Not Game Logic

A scene should contain visual elements, input handling, animation, and
presentation.

A scene should not contain damage calculations, artifact rules, or spell logic.

## Node Scripts

Keep scripts small.

Good:

```
PlayerView.cs
- Movement input
- Animation updates
```

Bad:

```
Player.cs
- Movement
- Combat
- Inventory
- Saving
- UI
- Effects
```

## UI Standards

UI displays information. UI does not decide game rules.

```
Bad:   Button: "Apply damage"
Good:  Button: "Request attack action"
```

The combat system decides the result.

---

# Content Creation Standards

Every artifact should answer: **what strategy does this enable?**

```
Bad:   "Increase damage by 5%"
Good:  "Fire damage increases every turn until the player takes damage."
```

Every new artifact should specify:

- Name
- Description
- Tags
- Trigger
- Effects
- Intended playstyle
- Possible synergies

---

# Git Standards

## Commit Messages

Good:

```
Add artifact trigger system
Implement damage effect resolution
Create fire rune data format
```

Bad:

```
stuff
changes
update
```

## Branches

```
main

feature/artifact-system
feature/combat-grid
feature/workshop-ui
```

---

# AI Development Rules

When an AI agent modifies the project:

**Before coding:**

1. Read relevant documentation.
2. Understand existing architecture.
3. Identify reusable systems.
4. Avoid adding unnecessary complexity.

**After coding:**

1. Explain design decisions.
2. Add tests when appropriate.
3. Update documentation if architecture changes.
4. Confirm scope remains aligned.

See `AGENTS.md` for the full agent contract.

---

# Scope Protection

During prototype development, do not add:

- Multiplayer
- Online features
- Large open world
- Complex dialogue systems
- Procedural AI content generation
- Economy systems
- Advanced crafting

The prototype exists to prove one thing:

> "Exploring, discovering artifacts, and creating builds is fun."

---

# Final Decision Rule

When choosing between two approaches, choose the one that creates:

- More player discovery
- More meaningful combinations
- More reusable systems
- Less unnecessary complexity

The goal is not to build the biggest game. The goal is to build the foundation
of a great one.
