using System;
using System.Collections.Generic;
using Script.Inventory.Controller;
using Script.Items;
using UnityEngine;

namespace Script.Entities
{
    public abstract class Character : MonoBehaviour
    {
        [Header("Character Information")]
        [SerializeField] protected string characterName = "New Character";
        
        [Header("Primary Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float baseDefense = 5f;
        [SerializeField] protected float baseMoveSpeed = 5f;
        [SerializeField] protected float baseAttackCooldown = 1f;
        
        public string CharacterName => characterName;
        private float CurrentHealth { get; set; }

        protected bool IsDead
        {
            get => isDead;
            private set => isDead = value;
        }

        public float HealthPercentage => maxHealth > 0 ? CurrentHealth / maxHealth : 0;

        [Header("Inventory & Equipment")]
        [SerializeField] protected InventoryController inventory;
        
        private readonly Dictionary<EquipSlot, Equipment> _equippedItems = new Dictionary<EquipSlot, Equipment>();
        
        protected float AttackTimer;
        [SerializeField] private bool isDead;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDie;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (AttackTimer > 0)
            {
                AttackTimer -= Time.deltaTime;
            }
        }

        #region Combat & Health Logic

        public virtual void TakeDamage(float amount)
        {
            if (IsDead) 
                return;
            var totalDefense = GetTotalDefense();
            var finalDamage = Mathf.Max(0, amount - totalDefense);
            
            CurrentHealth -= finalDamage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            OnHealthChanged?.Invoke(CurrentHealth);
            Debug.Log($"[Combat] {name} nhận {finalDamage} sát thương. Máu còn: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Heal(float amount)
        {
            if (IsDead) return;
            
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        protected virtual void Die()
        {
            if (IsDead) return;
            
            IsDead = true;
            OnDie?.Invoke();
            Debug.Log($"[Entity] {characterName} đã gục ngã.");
        }
        
        protected virtual bool CanAttack()
        {
            return !IsDead && AttackTimer <= 0;
        }

        public abstract void Attack();

        #endregion

        #region Stats & Equipment
        
        protected virtual float GetTotalDamage()
        {
            var total = baseDamage;
            
            if (_equippedItems.TryGetValue(EquipSlot.MainHand, out var equipment) && equipment is Tool weapon)
            {
                total += weapon.damage;
            }
            
            return total;
        }
        protected virtual float GetTotalDefense()
        {
            var total = baseDefense;
            
            foreach (var item in _equippedItems.Values)
            {
                if (item is Armor armor)
                {
                    total += armor.defense;
                }
            }
            
            return total;
        }
        
        protected virtual float GetMoveSpeed()
        {
            float totalPenalty = 0;
            foreach (var item in _equippedItems.Values)
            {
                if (item is Armor armor)
                {
                    totalPenalty += armor.movementSpeedPenalty;
                }
            }
            
            return Mathf.Max(1f, baseMoveSpeed - totalPenalty);
        }

        public virtual bool Equip(Equipment equipment)
        {
            if (equipment == null) return false;
            
            if (_equippedItems.ContainsKey(equipment.equipSlot))
            {
                Unequip(equipment.equipSlot);
            }

            _equippedItems[equipment.equipSlot] = equipment;
            Debug.Log($"[Inventory] {characterName} đã trang bị: {equipment.ItemName}");
            return true;
        }

        protected virtual bool Unequip(EquipSlot slot)
        {
            return _equippedItems.Remove(slot, out _);
        }

        #endregion
    }
}