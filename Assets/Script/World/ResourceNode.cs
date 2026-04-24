using System;
using Script.Interfaces;
using Script.Items;
using UnityEngine;

namespace Script.World
{
    public class ResourceNode : MonoBehaviour, IInteractable, IDamageable
    {
        [Header("Setting")]
        [SerializeField] private float staminaCostPerHit = 5f;
        [SerializeField] private string resourceName;
        [SerializeField] private float maxHealth = 3;
        [SerializeField] private ToolType requiredTool = ToolType.Pickaxe;
        
        [Header("Animation")]
        [SerializeField] private string interactAnimTrigger = "";
        
        public string InteractionAnimationTrigger => interactAnimTrigger;

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

        public void TakeDamage(float amount, Entities.Character source = null)
        {
            if (IsDead) return;

            CurrentHealth -= amount;
            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            
            // Visual feedback (đã chuyển sang Awake/Update hoặc có thể dùng Tween)
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

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 10f);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void Interact(Entities.Character interactor)
        {
            float damage = 1;
            
            if (interactor is IInventoryHolder holder)
            {
                var mainItem = interactor.GetComponent<Entities.EquipmentManager>()?.GetEquippedItem(EquipSlot.MainHand);
                if (mainItem is Tool tool)
                {
                    if (tool.toolType == requiredTool)
                    {
                        damage = tool.damage;
                    }
                }
            }

            TakeDamage(damage, interactor);
        }
    }
}