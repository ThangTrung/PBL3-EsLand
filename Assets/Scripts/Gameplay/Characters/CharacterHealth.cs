using System;
using Core.Contracts.Equipment;
using Core.Contracts.Combat;
using System.Linq;
using UnityEngine;

namespace Gameplay.Characters
{
    public class CharacterHealth : MonoBehaviour, IDamageable
    {
        [Header("Primary Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float baseDefense = 5f;

        public float MaxHealth 
        {
            get 
            {
                var total = maxHealth;
                if (_equipmentController != null)
                    total += _equipmentController.GetTotalHealthModifier();
                return total;
            }
        }

        public float TotalDefense
        {
            get
            {
                var total = baseDefense;
                if (_equipmentController != null)
                    total += _equipmentController.GetTotalDefenseModifier();
                return total;
            }
        }

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action<float, Character> OnDamageTaken;
        public event Action OnDie;

        private IEquipmentController _equipmentController;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
        }

        private void Start()
        {
            _equipmentController = GetComponent<IEquipmentController>();
            // Re-initialize to account for equipment modifiers if they are applied before/during Start
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead) return;

            float originalDamage = amount;

            // Apply Damage Modifiers
            var modifiers = GetComponents<IDamageModifier>().OrderBy(m => m.Priority);
            foreach (var modifier in modifiers)
            {
                amount = modifier.ModifyDamage(amount, source);
                if (amount <= 0) break;
            }

            float currentDefense = TotalDefense;
            var finalDamage = Mathf.Max(0, amount - currentDefense);
            CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);

            OnDamaged?.Invoke();
            OnDamageTaken?.Invoke(finalDamage, source);
            OnHealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDie?.Invoke();
        }

        public void SetCurrentHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
            IsDead = CurrentHealth <= 0;
            OnHealthChanged?.Invoke(CurrentHealth);
            if (IsDead) OnDie?.Invoke();
        }

        public void SetMaxHealth(float value, bool resetCurrent = true)
        {
            maxHealth = value;
            if (!resetCurrent) return;
            CurrentHealth = MaxHealth;
            IsDead = false;
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
}
