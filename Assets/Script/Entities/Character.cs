using System;
using Script.Equipment.Core;
using Script.Equipment.Interfaces;
using Script.Inventory.Interfaces;
using Script.Shared.Interfaces;
using UnityEngine;

namespace Script.Entities
{
    public abstract class Character : MonoBehaviour, IDamageable, IInventoryHolder
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
        [SerializeField] public Animator animator;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected RuntimeAnimatorController baseAnimator;

        public virtual Script.Inventory.Interfaces.IInventory Inventory => GetComponentInChildren<Script.Inventory.Interfaces.IInventory>();
        public virtual IEquipmentManager EquipmentManager => GetComponent<IEquipmentManager>();

        public string CharacterName => characterName;
        public float HealthPercentage
        {
            get
            {
                var max = GetMaxHealth();
                return max > 0 ? CurrentHealth / max : 0;
            }
        }

        public float MaxHealth => GetMaxHealth();
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        protected float AttackTimer;

        public event Action<float> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDie;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;

            if (animator && !baseAnimator)
                baseAnimator = animator.runtimeAnimatorController;
        }

        public virtual void ResetAnimator()
        {
            if (animator && baseAnimator)
                animator.runtimeAnimatorController = baseAnimator;
        }

        protected virtual void Update()
        {
            if (AttackTimer > 0)
                AttackTimer -= Time.deltaTime;
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

        protected void ExecuteAnimation(string animationTrigger)
        {
            if (animator && !string.IsNullOrEmpty(animationTrigger))
                animator.SetTrigger(animationTrigger);
        }

        #region Combat & Health Logic
        public virtual void TakeDamage(float amount, Character source = null)
        {
            if (IsDead) return;

            var finalDamage = Mathf.Max(0, amount - GetTotalDefense());
            CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0, GetMaxHealth());

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0)
                Die();
        }

        protected virtual void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, GetMaxHealth());
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
        public virtual float GetTotalDamage() => baseDamage;
        protected virtual float GetMaxHealth() => maxHealth;
        protected virtual float GetTotalDefense() => baseDefense;
        protected virtual float GetMoveSpeed() => Mathf.Max(1f, baseMoveSpeed);
        #endregion
    }
}
