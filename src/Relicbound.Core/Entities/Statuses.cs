using System.Collections.Generic;
using System.Linq;

namespace Relicbound.Core.Entities;

/// <remarks>
/// Stacking rule: applying a status already present sums stacks and refreshes
/// duration to the longer of the two (never shortens a status by reapplying
/// it). See docs/TECHNICAL_ARCHITECTURE.md section 3.
/// </remarks>
public sealed class Statuses : IComponent
{
    private readonly List<StatusInstance> _active = new();

    public IReadOnlyList<StatusInstance> Active => _active;

    public StatusInstance? Get(StatusType type)
    {
        return _active.FirstOrDefault(s => s.Type == type);
    }

    public bool Has(StatusType type) => Get(type) is not null;

    /// <returns>true if this stacked onto an existing status, false if newly applied.</returns>
    public bool Add(StatusType type, int stacks, int durationRounds)
    {
        var index = _active.FindIndex(s => s.Type == type);
        if (index < 0)
        {
            _active.Add(new StatusInstance(type, stacks, durationRounds));
            return false;
        }

        var existing = _active[index];
        _active[index] = existing with
        {
            Stacks = existing.Stacks + stacks,
            RemainingRounds = existing.RemainingRounds > durationRounds
                ? existing.RemainingRounds
                : durationRounds,
        };

        return true;
    }

    /// <returns>the statuses that expired this tick.</returns>
    public IReadOnlyList<StatusInstance> TickDurations()
    {
        var expired = new List<StatusInstance>();

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var status = _active[i];
            var remaining = status.RemainingRounds - 1;

            if (remaining <= 0)
            {
                expired.Add(status);
                _active.RemoveAt(i);
            }
            else
            {
                _active[i] = status with { RemainingRounds = remaining };
            }
        }

        return expired;
    }
}
