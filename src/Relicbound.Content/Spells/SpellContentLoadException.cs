using System;
using System.Collections.Generic;

namespace Relicbound.Content.Spells;

public sealed class SpellContentLoadException : Exception
{
    public SpellContentLoadException(IReadOnlyList<string> errors)
        : base("Spell content failed validation:" + Environment.NewLine + string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
