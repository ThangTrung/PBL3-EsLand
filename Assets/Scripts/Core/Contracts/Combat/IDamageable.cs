using System;
using Gameplay.Characters;

namespace Core.Contracts.Combat
{
    public interface IDamageable
    {
        float MaxHealth { get; }
        float CurrentHealth { get; }
        bool IsDead { get; }
        event Action<float> OnHealthChanged;
        event Action OnDamaged;
        event Action<float, Character> OnDamageTaken;

        event Action OnDie;
        void TakeDamage(float amount, Character source = null);
    }
}


