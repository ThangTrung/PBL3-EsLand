using System;
using Gameplay.Characters;

namespace Core.Contracts.Combat
{
    /// <summary>
    /// Giao diện cho các đối tượng có thể nhận sát thương và có Máu.
    /// Dùng cho cả Player, Enemy và các vật thể môi trường có thể phá hủy.
    /// </summary>
    public interface IDamageable
    {
        float MaxHealth { get; }
        float CurrentHealth { get; }
        bool IsDead { get; }

        /// <summary> Sự kiện khi máu thay đổi (trả về giá trị máu hiện tại) </summary>
        event Action<float> OnHealthChanged;

        /// <summary> Sự kiện cơ bản khi bị trúng đòn </summary>
        event Action OnDamaged;

        /// <summary> Sự kiện chi tiết khi bị trúng đòn (lượng sát thương và đối tượng gây ra) </summary>
        event Action<float, Character> OnDamageTaken;

        /// <summary> Sự kiện khi máu về 0 </summary>
        event Action OnDie;

        /// <summary>
        /// Thực hiện việc trừ máu của đối tượng.
        /// </summary>
        /// <param name="amount">Lượng sát thương</param>
        /// <param name="source">Đối tượng gây ra sát thương (có thể null)</param>
        void TakeDamage(float amount, Character source = null);
    }
}


