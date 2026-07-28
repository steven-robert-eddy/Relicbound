# Effects

`Effect`, `EffectContext`, `EffectResult`, the resolution loop, and the primitive effects (`DamageEffect`, `HealEffect`, `ApplyStatusEffect`, `ModifyStatEffect`, `MoveEffect`, `SummonEffect`).

> Every gameplay change is an Effect. An effect never executes another effect directly — it returns effects to be queued, which is what makes the recursion guards possible.

See [`docs/CODING_STANDARDS.md`](../../../docs/CODING_STANDARDS.md) for the rules that govern this folder.
