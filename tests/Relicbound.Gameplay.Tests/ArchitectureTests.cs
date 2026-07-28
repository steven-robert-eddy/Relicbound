using System;
using System.Linq;
using Relicbound.Gameplay;
using Xunit;

namespace Relicbound.Gameplay.Tests;

/// <summary>
/// Enforces that gameplay systems stay engine-free, so combat can be simulated
/// headlessly in tests and on CI.
/// </summary>
public class ArchitectureTests
{
    [Fact]
    public void Gameplay_DoesNotDependOnGodot()
    {
        var offenders = GameplayAssembly.Reference
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => name.Contains("Godot", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Relicbound.Gameplay must run without the engine, but it references: "
                + string.Join(", ", offenders)
                + ". Presentation belongs in the game project.");
    }

    [Fact]
    public void Gameplay_DoesNotDependOnThePresentationLayer()
    {
        var offenders = GameplayAssembly.Reference
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => string.Equals(name, "Relicbound.Game", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Relicbound.Gameplay must not reference the Godot game project. "
                + "Scenes observe gameplay; gameplay never reaches into scenes.");
    }
}
