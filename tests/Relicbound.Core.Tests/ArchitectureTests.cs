using System;
using System.Linq;
using Relicbound.Core;
using Xunit;

namespace Relicbound.Core.Tests;

/// <summary>
/// Enforces the project's central architectural rule: the simulation must be
/// runnable without the engine.
/// </summary>
/// <remarks>
/// <para>
/// Every other rule in <c>docs/CODING_STANDARDS.md</c> is a convention that a
/// careless change can quietly erode. This one is checked on every CI run.
/// </para>
/// <para>
/// Caveat worth knowing: <see cref="System.Reflection.Assembly.GetReferencedAssemblies"/>
/// reports what the compiler recorded in the manifest, which covers assemblies
/// actually used — not unused entries in the csproj. That is the behaviour we
/// want. A dangling reference nobody uses is harmless; the moment someone
/// writes <c>using Godot;</c> and touches a Godot type, this test goes red.
/// </para>
/// </remarks>
public class ArchitectureTests
{
    [Fact]
    public void Core_DoesNotDependOnGodot()
    {
        var offenders = CoreAssembly.Reference
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => name.Contains("Godot", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Relicbound.Core must run without the engine, but it references: "
                + string.Join(", ", offenders)
                + ". Move anything that needs Godot into the game project.");
    }

    [Fact]
    public void Core_DoesNotDependOnHigherLayers()
    {
        // Dependencies flow downward: Game -> Content -> Gameplay -> Core.
        // Core sits at the bottom and must not reach back up.
        string[] higherLayers = ["Relicbound.Gameplay", "Relicbound.Content", "Relicbound.Game"];

        var offenders = CoreAssembly.Reference
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => higherLayers.Contains(name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Relicbound.Core is the bottom layer, but it references: "
                + string.Join(", ", offenders)
                + ". Dependencies must flow downward only.");
    }
}
