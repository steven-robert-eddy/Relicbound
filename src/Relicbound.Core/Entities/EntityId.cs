namespace Relicbound.Core.Entities;

public readonly record struct EntityId(int Value)
{
    public override string ToString() => $"Entity#{Value}";
}
