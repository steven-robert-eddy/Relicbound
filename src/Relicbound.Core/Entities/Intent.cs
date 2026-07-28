using Relicbound.Core.Rules;

namespace Relicbound.Core.Entities;

/// <remarks>
/// What an entity has committed to doing this round, set before the
/// player's turn and executed after it — this is what makes telegraphing
/// real: once declared, an intent resolves as committed, not re-evaluated
/// against however the board has changed by the time it executes.
/// </remarks>
public sealed class Intent : IComponent
{
    public IntentKind Kind { get; private set; } = IntentKind.None;
    public GridPoint? TargetPosition { get; private set; }
    public EntityId? TargetEntityId { get; private set; }

    public void SetMove(GridPoint targetPosition)
    {
        Kind = IntentKind.Move;
        TargetPosition = targetPosition;
        TargetEntityId = null;
    }

    public void SetAttack(EntityId targetEntityId)
    {
        Kind = IntentKind.Attack;
        TargetEntityId = targetEntityId;
        TargetPosition = null;
    }

    public void Clear()
    {
        Kind = IntentKind.None;
        TargetPosition = null;
        TargetEntityId = null;
    }
}
