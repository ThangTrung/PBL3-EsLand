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

        public float MaxHealth => CalculateMaxHealth();
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action<float, Character> OnDamageTaken;
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

            float originalDamage = amount;

            // Apply Damage Modifiers
            var modifiers = GetComponents<IDamageModifier>().OrderBy(m => m.Priority);
            foreach (var modifier in modifiers)
            {
                amount = modifier.ModifyDamage(amount, source);
                if (amount <= 0) break;
            }

            float totalDefense = CalculateTotalDefense();
            var finalDamage = Mathf.Max(0, amount - totalDefense);
            CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0, CalculateMaxHealth());

            Debug.Log($"[Health] {gameObject.name} nhận sát thương từ {(source != null ? source.name : "Unknown")}. " +
                      $"Gốc: {originalDamage}, Sau Mod: {amount}, Giáp: {totalDefense} => Sát thương thực: {finalDamage}. " +
                      $"Máu hiện tại: {CurrentHealth}/{MaxHealth}");

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, CalculateMaxHealth());
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDie?.Invoke();
        }

        public void SetMaxHealth(float value, bool resetCurrent = true)
        {
            maxHealth = value;
            if (!resetCurrent) return;
            CurrentHealth = maxHealth;
            IsDead = false;
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private float CalculateMaxHealth()
        {
            var total = maxHealth;
            if (_equipmentController != null)
                total += _equipmentController.GetTotalHealthModifier();
            return total;
        }

        private float CalculateTotalDefense()
        {
            var total = baseDefense;
            if (_equipmentController != null)
                total += _equipmentController.GetTotalDefenseModifier();
            return total;
        }
    }
}