namespace Relicbound.Core.Entities;

public sealed class TurnResources : IComponent
{
    public TurnResources(int actionPoints)
    {
        ActionPoints = actionPoints;
    }

    public int ActionPoints { get; set; }
}
