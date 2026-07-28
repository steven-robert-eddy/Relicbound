namespace Relicbound.Core.Entities;

/// <remarks>
/// A marker component, not a gameplay Tag (Fire/Ice/...). It exists so
/// systems can ask "is this the player" without hardcoding a name or id.
/// </remarks>
public sealed class PlayerControlled : IComponent
{
}
