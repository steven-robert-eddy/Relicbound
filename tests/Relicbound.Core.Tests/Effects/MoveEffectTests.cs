using Relicbound.Core.Effects;
using Relicbound.Core.Entities;
using Relicbound.Core.Events;
using Relicbound.Core.Rules;
using Xunit;

namespace Relicbound.Core.Tests.Effects;

public class MoveEffectTests
{
    [Fact]
    public void Execute_UpdatesGridPosition()
    {
        var entity = new Entity(new EntityId(1), "Player");
        entity.Add(new GridPosition(new GridPoint(0, 0)));
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(entity, new[] { entity }, random, depth: 0, EffectOrigin.Direct);

        new MoveEffect(new GridPoint(1, 0)).Execute(context);

        Assert.Equal(new GridPoint(1, 0), entity.Get<GridPosition>()!.Point);
    }

    [Fact]
    public void Execute_PublishesMovedEvent()
    {
        var entity = new Entity(new EntityId(1), "Player");
        entity.Add(new GridPosition(new GridPoint(0, 0)));
        var random = new SplitMix64RandomSource(1);
        var context = new EffectContext(entity, new[] { entity }, random, depth: 0, EffectOrigin.Direct);

        var result = new MoveEffect(new GridPoint(2, 3)).Execute(context);

        var movedEvent = Assert.Single(result.Events);
        var moved = Assert.IsType<MovedEvent>(movedEvent);
        Assert.Equal(new GridPoint(0, 0), moved.From);
        Assert.Equal(new GridPoint(2, 3), moved.To);
    }
}
