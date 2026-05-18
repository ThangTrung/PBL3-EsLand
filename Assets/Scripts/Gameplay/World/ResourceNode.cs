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
    public class ResourceNode : MonoBehaviour, IInteractable, IDamageable, Infrastructure.SaveSystem.Core.ISaveable
    {
        [Header("Resource Settings")]
        [SerializeField] private float maxHealth = 3f;
        [SerializeField] private ToolType requiredTool = ToolType.None;
        [SerializeField] private float staminaCostPerHit = 5f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDie;

        private Animator _animator;
        private static readonly int HitHash = Animator.StringToHash("Hit");
        
        // Cần ID từ SaveableEntity để lưu trữ
        private string _entityId;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            _animator = GetComponent<Animator>();
            
            if (TryGetComponent<Infrastructure.SaveSystem.Core.SaveableEntity>(out var entity))
            {
                _entityId = entity.Id;
            }
        }

        // --- ISaveable Implementation ---
        public void LoadData(Infrastructure.SaveSystem.Data.GameData data)
        {
            if (string.IsNullOrEmpty(_entityId)) return;

            var nodeData = data.resources.Find(r => r.resourceID == _entityId);
            if (nodeData != null)
            {
                if (nodeData.isDestroyed)
                {
                    gameObject.SetActive(false); // Hoặc Destroy ngay
                    return;
                }
                CurrentHealth = nodeData.currentHealth;
            }
        }

        public void SaveData(Infrastructure.SaveSystem.Data.GameData data)
        {
            if (string.IsNullOrEmpty(_entityId)) return;

            data.resources.RemoveAll(r => r.resourceID == _entityId);
            data.resources.Add(new Infrastructure.SaveSystem.Data.ResourceNodeSaveData
            {
                resourceID = _entityId,
                isDestroyed = IsDead,
                currentHealth = CurrentHealth
            });
        }
        // ...

        public void Interact(Character interactor)
        {
            if (IsDead) return;

            float finalDamage = CalculateDamage(interactor);
            if (!ValidateTool(interactor, ref finalDamage)) return;

            Debug.Log($"Applying {finalDamage} damage to {gameObject.name}");
            TakeDamage(finalDamage, interactor);
        }

        private float CalculateDamage(Character interactor)
        {
            if (interactor.TryGetComponent<PlayerInteractionController>(out var pic))
                return pic.GetTotalDamage();
            return 1f;
        }

        private bool ValidateTool(Character interactor, ref float damage)
        {
            if (requiredTool == ToolType.None) return true;

            var equipment = interactor.EquipmentManager;
            if (equipment == null)
            {
                Debug.LogWarning($"[ResourceNode] {interactor.CharacterName} has no EquipmentManager!");
                return false;
            }

            var mainItem = equipment.GetEquippedItem(EquipSlot.MainHand);

            if (mainItem is IGatheringTool tool && tool.Type == requiredTool)
            {
                float baseToolDmg = (mainItem is IWeapon weapon) ? weapon.Damage : 1f;
                damage = baseToolDmg * tool.GatherSpeedMultiplier;
                return true;
            }

            Debug.Log($"[ResourceNode] Wrong tool or no tool equipped! Need: {requiredTool}. Equipped: {mainItem?.GetType().Name ?? "None"}");
            return false;
        }

        public void TakeDamage(float amount, Character source = null)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            
            // Visual feedback: Animator Hit only
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                if (HasParameter(HitHash)) _animator.SetTrigger(HitHash);
            }

            if (CurrentHealth <= 0) Die();
        }

        private bool HasParameter(int paramHash)
        {
            foreach (var param in _animator.parameters)
                if (param.nameHash == paramHash) return true;
            return false;
        }

        protected virtual void Die()
        {
            IsDead = true;
            OnDie?.Invoke();
            
            if (TryGetComponent<LootSpawner>(out var spawner))
                spawner.SpawnLoot();
            
            Destroy(gameObject);
        }

        public float GetStaminaCost() => staminaCostPerHit;
    }
}

