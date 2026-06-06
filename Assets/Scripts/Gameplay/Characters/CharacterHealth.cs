using System;
using Core.Contracts.Equipment;
using Core.Contracts.Combat;
using Data.Equipment;
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
        private IDamageModifier[] _cachedModifiers;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
            RefreshModifiers();
        }

        private void Start()
        {
            _equipmentController = GetComponent<IEquipmentController>();
            // REMOVED: CurrentHealth = MaxHealth; -> Để cho hệ thống Save/Load hoặc khởi tạo thủ công quyết định

            // Đăng ký sự kiện thay đổi trang bị để cập nhật máu
            if (_equipmentController != null)
            {
                _equipmentController.OnItemEquipped += HandleEquipmentChanged;
                _equipmentController.OnItemUnequipped += HandleEquipmentChanged;
            }
        }

        private void OnDestroy()
        {
            if (_equipmentController != null)
            {
                _equipmentController.OnItemEquipped -= HandleEquipmentChanged;
                _equipmentController.OnItemUnequipped -= HandleEquipmentChanged;
            }
        }

        private void HandleEquipmentChanged(EquipSlot slot, IEquippable item)
        {
            // Khi tháo lắp đồ, MaxHealth tự động thay đổi nhờ vào Getter MaxHealth.
            // Chúng ta chỉ cần đảm bảo CurrentHealth không bị tràn và cập nhật UI.
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        public void RefreshModifiers()
        {
            _cachedModifiers = GetComponents<IDamageModifier>().OrderBy(m => m.Priority).ToArray();
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead) return;

            float originalDamage = amount;

            // Apply Damage Modifiers using cached list
            if (_cachedModifiers != null)
            {
                foreach (var modifier in _cachedModifiers)
                {
                    amount = modifier.ModifyDamage(amount, source);
                    if (amount <= 0) break;
                }
            }

            float currentDefense = TotalDefense;
            var finalDamage = Mathf.Max(0, amount - currentDefense);
            CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);

            OnDamaged?.Invoke();
            OnDamageTaken?.Invoke(finalDamage, source);
            OnHealthChanged?.Invoke(CurrentHealth);

            // [NEW] Trigger Camera Shake and Effects
            if (finalDamage > 0)
            {
                if (CompareTag("Player"))
                {
                    if (Gameplay.World.CameraShake.Instance != null)
                        Gameplay.World.CameraShake.Instance.Shake(0.2f, 0.1f);
                }
                
                if (Gameplay.World.EffectManager.Instance != null)
                    Gameplay.World.EffectManager.Instance.PlayHitEffect(transform.position);
            }

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

            // [NEW] Trigger Death Effect
            if (Gameplay.World.EffectManager.Instance != null)
                Gameplay.World.EffectManager.Instance.PlayDeathEffect(transform.position);

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
