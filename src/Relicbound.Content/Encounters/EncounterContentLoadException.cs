using System;
using System.Collections.Generic;

namespace Relicbound.Content.Encounters;

public sealed class EncounterContentLoadException : Exception
{
    public EncounterContentLoadException(IReadOnlyList<string> errors)
        : base("Encounter content failed validation:" + Environment.NewLine + string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
