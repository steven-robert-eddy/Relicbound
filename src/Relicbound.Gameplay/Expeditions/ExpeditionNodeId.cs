namespace Relicbound.Gameplay.Expeditions;

public readonly record struct ExpeditionNodeId(int Value)
{
    public override string ToString() => $"Node#{Value}";
}
