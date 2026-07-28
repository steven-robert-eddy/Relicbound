using System;

namespace Relicbound.Core.Entities;

public sealed class Health : IComponent
{
    public Health(int maxHealth)
    {
        MaxHealth = maxHealth;
        Current = maxHealth;
    }

    public int MaxHealth { get; private set; }
    public int Current { get; private set; }
    public int Shield { get; private set; }
    public bool IsDefeated => Current <= 0;

    public int ApplyDamage(int amount)
    {
        if (amount <= 0) { return 0; }

        var absorbedByShield = Math.Min(Shield, amount);
        Shield -= absorbedByShield;

        var remaining = amount - absorbedByShield;
        var actualDamage = Math.Min(Current, remaining);
        Current -= actualDamage;

        return actualDamage;
    }

    public int Heal(int amount)
    {
        if (amount <= 0) { return 0; }

        var actualHeal = Math.Min(MaxHealth - Current, amount);
        Current += actualHeal;
        return actualHeal;
    }

    public void AddShield(int amount)
    {
        if (amount <= 0) { return; }
        Shield += amount;
    }
}
