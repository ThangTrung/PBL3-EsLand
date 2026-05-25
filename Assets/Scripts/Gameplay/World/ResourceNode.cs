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

        [Header("Visual Settings (New Stump System)")]
        [SerializeField] private Sprite stumpSprite;      // Kéo trực tiếp hình cái GỐC CÂY vào đây
        [SerializeField] private Collider2D nodeCollider; // Kéo BoxCollider2D hoặc PolygonCollider2D của cây vào đây

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action<float, Character> OnDamageTaken;
        public event Action OnDie;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer; // Thêm để quản lý hình ảnh
        private Sprite _defaultTreeSprite;      // Thêm để lưu lại hình cái cây ban đầu
        private static readonly int HitHash = Animator.StringToHash("Hit");
        
        private string _entityId;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); // Lấy SpriteRenderer gốc của thằng cha

            // Lưu lại hình ảnh cái cây ban đầu để lúc load game còn biết đường vẽ lại
            if (_spriteRenderer != null)
            {
                _defaultTreeSprite = _spriteRenderer.sprite;
            }
            
            if (TryGetComponent<Infrastructure.SaveSystem.Core.SaveableEntity>(out var entity))
            {
                _entityId = entity.Id;
            }

            UpdateVisuals();
        }

        // --- ISaveable Implementation ---
        public void LoadData(Infrastructure.SaveSystem.Data.GameData data)
        {
            if (string.IsNullOrEmpty(_entityId)) return;

            var nodeData = data.resourceNodes.Find(r => r.nodeID == _entityId);
            
            if (nodeData != null)
            {
                CurrentHealth = nodeData.currentHP;
                IsDead = nodeData.isStump;
            }
            else
            {
                CurrentHealth = maxHealth;
                IsDead = false;
            }

            UpdateVisuals();
        }

        public void SaveData(Infrastructure.SaveSystem.Data.GameData data)
        {
            if (string.IsNullOrEmpty(_entityId)) return;

            data.resourceNodes.RemoveAll(r => r.nodeID == _entityId);

            if (CurrentHealth < maxHealth || IsDead)
            {
                data.resourceNodes.Add(new Infrastructure.SaveSystem.Data.ResourceNodeSaveData
                {
                    nodeID = _entityId,
                    isStump = IsDead,
                    currentHP = CurrentHealth
                });
            }
        }

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
            OnDamageTaken?.Invoke(amount, source);
            OnHealthChanged?.Invoke(CurrentHealth);
            
            if (_animator != null && _animator.runtimeAnimatorController != null && _animator.enabled)
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
            if (IsDead) return;

            IsDead = true;
            OnDie?.Invoke();
            
            if (TryGetComponent<LootSpawner>(out var spawner))
                spawner.SpawnLoot();
            
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = IsDead ? stumpSprite : _defaultTreeSprite;
            }
            
            if (_animator != null) 
            {
                _animator.enabled = !IsDead;
            }

            if (nodeCollider != null) 
            {
                nodeCollider.enabled = !IsDead;
            }
        }
        
        public bool HasRequiredTool(Character interactor)
        {
            if (requiredTool == ToolType.None) return true;
            
            if (interactor == null || interactor.EquipmentManager == null) return false;

            var mainItem = interactor.EquipmentManager.GetEquippedItem(EquipSlot.MainHand);
            if (mainItem is IGatheringTool tool && tool.Type == requiredTool)
            {
                return true;
            }
            
            return false;
        }
        public float GetStaminaCost() => staminaCostPerHit;
    }
}