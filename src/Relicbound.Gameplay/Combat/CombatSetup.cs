using System.Collections.Generic;
using Relicbound.Core.Entities;

namespace Relicbound.Gameplay.Combat;

public sealed record CombatSetup(int GridWidth, int GridHeight, IReadOnlyList<Entity> Entities, ulong RandomSeed);
