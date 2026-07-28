using System;
using System.Collections.Generic;

namespace Relicbound.Content.Artifacts;

public sealed class ArtifactContentLoadException : Exception
{
    public ArtifactContentLoadException(IReadOnlyList<string> errors)
        : base("Artifact content failed validation:" + Environment.NewLine + string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
