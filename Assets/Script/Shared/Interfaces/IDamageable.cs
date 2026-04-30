using System;

namespace Script.Shared.Interfaces
{
    public interface IDamageable
    {
        float MaxHealth { get; }
        float CurrentHealth { get; }
        bool IsDead { get; }
        event Action<float> OnHealthChanged;
        event Action OnDamaged;
        event Action OnDie;
        void TakeDamage(float amount, Script.Entities.Character source = null);
    }
}
