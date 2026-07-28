namespace Relicbound.Core.Effects;

public abstract class Effect
{
    public abstract EffectResult Execute(EffectContext context);
}
