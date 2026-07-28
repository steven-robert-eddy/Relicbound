using System.Collections.Generic;
using Relicbound.Core.Entities;

namespace Relicbound.Core.Effects;

public sealed record QueuedEffect(Effect Effect, IReadOnlyList<Entity> Targets);
