using System.Reflection;

namespace Relicbound.Gameplay;

/// <summary>
/// Anchor type used to locate the <c>Relicbound.Gameplay</c> assembly by reflection.
/// </summary>
/// <remarks>
/// This exists so tests and content loaders can reach the Gameplay assembly
/// without depending on any particular game type. It holds no game state and
/// no rules.
/// </remarks>
public static class GameplayAssembly
{
    /// <summary>Gets the <c>Relicbound.Gameplay</c> assembly.</summary>
    public static Assembly Reference => typeof(GameplayAssembly).Assembly;
}
