using System;
using Core.Contracts.Combat;
using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Core.Types;
using Data.Equipment;
using Gameplay.Characters;
using Gameplay.Equipment;
using UnityEngine;

namespace Gameplay.World
{
    public class ResourceNode : MonoBehaviour, IInteractable, IDamageable
    {
        [Header("Resource Settings")]
        [SerializeField] private string resourceName = "";
        [SerializeField] private float maxHealth = 3f;
        [SerializeField] private ToolType requiredTool = ToolType.None;
        [SerializeField] private float staminaCostPerHit = 5f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDie;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            if (transform.localScale.x > 1.01f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void Interact(Character interactor)
        {
            if (IsDead) return;

            var finalDamage = 0f;
            var equipmentManager = interactor.GetComponent<EquipmentManager>();
            var mainItem = equipmentManager?.GetEquippedItem(EquipSlot.MainHand);
            
            if (requiredTool != ToolType.None)
            {
                if (mainItem is IGatheringTool tool && tool.Type == requiredTool)
                {
                    finalDamage = (mainItem is IWeapon weapon) ? weapon.Damage : 1f;
                    finalDamage *= tool.GatherSpeedMultiplier;
                }
                else
                {
                    return; 
                }
            }
            else
            {
                finalDamage = (mainItem is IWeapon w) ? w.Damage : 1f;
            }
            
            TakeDamage(finalDamage, interactor);
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth -= amount;
            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            
            transform.localScale = Vector3.one * 1.2f;

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            IsDead = true;
            OnDie?.Invoke();
            
            if (TryGetComponent<LootSpawner>(out var spawner))
            {
                spawner.SpawnLoot();
            }
            
            Destroy(gameObject);
        }

        public float GetStaminaCost() => staminaCostPerHit;
    }
}

