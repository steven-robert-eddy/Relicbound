using System;
using System.Collections.Generic;

namespace Relicbound.Content.Runes;

public sealed class RuneContentLoadException : Exception
{
    public RuneContentLoadException(IReadOnlyList<string> errors)
        : base("Rune content failed validation:" + Environment.NewLine + string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
