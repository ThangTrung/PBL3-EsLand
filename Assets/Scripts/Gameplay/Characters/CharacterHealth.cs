using System;
using Core.Contracts.Equipment;
using Core.Contracts.Combat;
using UnityEngine;

namespace Gameplay.Characters
{
    public class CharacterHealth : MonoBehaviour, IDamageable
    {
        [Header("Primary Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float baseDefense = 5f;

        public float MaxHealth => GetMaxHealth();
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDie;

        private IEquipmentController _equipmentController;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        private void Start()
        {
            _equipmentController = GetComponent<IEquipmentController>();
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead) return;

            var finalDamage = Mathf.Max(0, amount - GetTotalDefense());
            CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0, GetMaxHealth());

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, GetMaxHealth());
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDie?.Invoke();
        }

        private float GetMaxHealth()
        {
            var total = maxHealth;
            if (_equipmentController != null)
                total += _equipmentController.GetTotalHealthModifier();
            return total;
        }

        private float GetTotalDefense()
        {
            var total = baseDefense;
            if (_equipmentController != null)
                total += _equipmentController.GetTotalDefenseModifier();
            return total;
        }
    }
}
