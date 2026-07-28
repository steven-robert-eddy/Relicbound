using System.Collections.Generic;

namespace Relicbound.Core.Events;

/// <remarks>
/// One record of everything that happened, in resolution order. The combat
/// log, the animation feed, and test assertions all read from this. See
/// docs/TECHNICAL_ARCHITECTURE.md section 9.
/// </remarks>
public sealed class Journal
{
    private readonly List<IGameEvent> _entries = new();

    public IReadOnlyList<IGameEvent> Entries => _entries;

    public void Record(IGameEvent gameEvent)
    {
        _entries.Add(gameEvent);
    }
}
