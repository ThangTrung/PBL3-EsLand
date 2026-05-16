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

            var finalDamage = 1f; // Default base damage if no weapon logic applies
            var interactionController = interactor.GetComponent<PlayerInteractionController>();
            
            if (interactionController != null)
            {
                finalDamage = interactionController.GetTotalDamage();
            }

            var equipmentManager = interactor.GetComponent<EquipmentManager>() ?? interactor.GetComponentInChildren<EquipmentManager>();
            var mainItem = equipmentManager?.GetEquippedItem(EquipSlot.MainHand);
            
            Debug.Log($"Interacting with {resourceName}. Required tool: {requiredTool}. Main item: {mainItem?.GetType().Name ?? "None"}");

            if (requiredTool != ToolType.None)
            {
                if (mainItem is IGatheringTool tool && tool.Type == requiredTool)
                {
                    // For resources, we prioritize the tool's specialized gathering logic
                    float toolDamage = (mainItem is IWeapon weapon) ? weapon.Damage : 1f;
                    finalDamage = toolDamage * tool.GatherSpeedMultiplier;
                }
                else
                {
                    Debug.Log($"Wrong tool or no tool equipped! Need: {requiredTool}");
                    return; 
                }
            }
            
            Debug.Log($"Applying {finalDamage} damage to {gameObject.name} (Calculated from: {(mainItem != null ? "Tool" : "Base Interaction")})");
            TakeDamage(finalDamage, interactor);
        }

        private Animator _animator;
        private static readonly int HitHash = Animator.StringToHash("Hit");

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            
            // Visual feedback: Scaling pop
            transform.localScale = Vector3.one * 1.2f;

            // Visual feedback: Animator trigger
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                foreach (var param in _animator.parameters)
                {
                    if (param.nameHash == HitHash)
                    {
                        _animator.SetTrigger(HitHash);
                        break;
                    }
                }
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
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

