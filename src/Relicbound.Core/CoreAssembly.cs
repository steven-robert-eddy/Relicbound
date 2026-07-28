using System.Reflection;

namespace Relicbound.Core;

/// <summary>
/// Anchor type used to locate the <c>Relicbound.Core</c> assembly by reflection.
/// </summary>
/// <remarks>
/// This exists so tests and content loaders can reach the Core assembly without
/// depending on any particular game type. It holds no game state and no rules.
/// </remarks>
public static class CoreAssembly
{
    /// <summary>Gets the <c>Relicbound.Core</c> assembly.</summary>
    public static Assembly Reference => typeof(CoreAssembly).Assembly;
}
