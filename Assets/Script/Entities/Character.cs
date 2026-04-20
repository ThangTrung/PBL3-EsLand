using System;
using System.Collections.Generic;
using Script.Items;
using UnityEngine;

namespace Script.Entities
{
    public abstract class Character : MonoBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("isMoving"); 

        [Header("Character Information")]
        [SerializeField] protected string characterName = "New Character";
        
        [Header("Primary Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float baseDefense = 5f;
        [SerializeField] protected float baseMoveSpeed = 5f;
        [SerializeField] protected float baseAttackCooldown = 1f;
        
        [Header("Components")]
        [SerializeField] protected Animator animator; 
        [SerializeField] protected Rigidbody2D rb;
        
        public string CharacterName => characterName;
        public float HealthPercentage => maxHealth > 0 ? CurrentHealth / maxHealth : 0;

        private float CurrentHealth { get; set; }
        protected bool IsDead { get; private set; }
        protected float AttackTimer;
        
        private readonly Dictionary<EquipSlot, Equipment> _equippedItems = new Dictionary<EquipSlot, Equipment>();
        
        public event Action<float> OnHealthChanged;
        public event Action OnDie;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }
        protected virtual void Update()
        {
            if (AttackTimer > 0)
            {
                AttackTimer -= Time.deltaTime;
            }
        }

        protected virtual void Move(Vector3 direction)
        {
            if (IsDead) 
            {
                rb.velocity = Vector2.zero;
                return;
            }
            var movement = direction.normalized * GetMoveSpeed();
            rb.velocity = new Vector2(movement.x, movement.y);
            
            var isMoving = direction.sqrMagnitude > 0;
            animator.SetBool(IsMoving, isMoving);
            
            if (direction.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }

        #region Combat & Health Logic
        public virtual void TakeDamage(float amount)
        {
            if (IsDead) return;
            
            var finalDamage = Mathf.Max(0, amount - GetTotalDefense());
            CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0, maxHealth);

            OnHealthChanged?.Invoke(CurrentHealth);
            Debug.Log($"[Combat] {name} nhận {finalDamage} sát thương. Máu còn: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0) Die();
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
        }
        
        protected virtual bool CanAttack() => !IsDead && AttackTimer <= 0;
        
        public abstract void Attack();
        #endregion

        #region Stats & Equipment
        protected virtual float GetTotalDamage()
        {
            var total = baseDamage;
            if (_equippedItems.TryGetValue(EquipSlot.MainHand, out var equipment) && equipment is Tool weapon)
                total += weapon.damage;
            return total;
        }

        protected virtual float GetTotalDefense()
        {
            var total = baseDefense;
            foreach (var item in _equippedItems.Values)
                if (item is Armor armor) total += armor.defense;
            return total;
        }
        
        protected virtual float GetMoveSpeed()
        {
            float totalPenalty = 0;
            foreach (var item in _equippedItems.Values)
                if (item is Armor armor) totalPenalty += armor.movementSpeedPenalty;
            return Mathf.Max(1f, baseMoveSpeed - totalPenalty);
        }

        protected virtual void HandleEquip(Equipment equipment)
        {
            if (equipment == null) return;
            
            if (_equippedItems.ContainsKey(equipment.equipSlot))
            {
                HandleUnequip(equipment.equipSlot); 
            }

            _equippedItems[equipment.equipSlot] = equipment;
            Debug.Log($"[Inventory] {characterName} đã trang bị: {equipment.ItemName}");
        }

        protected virtual void HandleUnequip(EquipSlot slot)
        {
            if (_equippedItems.Remove(slot, out var removedItem))
            {
                Debug.Log($"[Inventory] {characterName} đã tháo: {removedItem.ItemName}");
            }
        }
        #endregion
    }
}