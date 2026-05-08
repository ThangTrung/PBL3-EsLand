using System.Collections.Generic;
using Script.Entities.AI;
using Script.Interfaces;
using UnityEngine;

namespace Script.Entities
{
    /// <summary>
    /// Enemy kế thừa Character: dùng stat/combat/cooldown từ Character,
    /// AI chỉ quyết định hành vi (patrol/chase/attack) thông qua State Pattern.
    /// </summary>
    public class Enemy : Character, IInteractable, IInventoryHolder
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private string targetTag = "Player";

        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float patrolReachDistance = 0.3f;

        private IEnemyState _currentState;

        // Implementation of the new interface property
        public string InteractionAnimationTrigger => "attack";

        // Public getters for AI properties
        public Transform Target => target;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float PatrolRadius => patrolRadius;
        public float PatrolReachDistance => patrolReachDistance;

        protected override void Awake()
        {
            base.Awake();

            if (target == null && !string.IsNullOrWhiteSpace(targetTag))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go != null) target = go.transform;
            }

            ChangeState(new EnemyPatrolState());
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            _currentState?.Execute(this);
        }

        public void ChangeState(IEnemyState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState?.Enter(this);
        }

        public void MoveTowardsPosition(Vector3 pos)
        {
            var dir = pos - transform.position;
            dir.z = 0f;

            if (dir.sqrMagnitude < 0.0001f)
            {
                Move(Vector3.zero);
                return;
            }

            dir.Normalize();
            Move(dir);
        }

        public void StopMovement()
        {
            Move(Vector3.zero);
        }

        public override void Attack()
        {
            if (!CanAttack()) return;
            if (!Target) return;

            var distance = Vector3.Distance(transform.position, Target.position);
            if (distance > attackRange) return;

            // Trigger animation from Character base class
            ExecuteAnimation(InteractionAnimationTrigger);

            if (Target.TryGetComponent<IDamageable>(out var victim))
                victim.TakeDamage(GetTotalDamage(), this);

            AttackTimer = baseAttackCooldown;
        }

        protected override void Die()
        {
            base.Die();

            Debug.Log($"[Enemy] {characterName} đã bị tiêu diệt.");

            var lootDropper = GetComponent<LootDropper>();
            if (lootDropper != null)
                lootDropper.Drop(characterName);

            // tuỳ game: có thể disable thay vì destroy (để object pool)
            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }

        public void Interact(Character interactor)
        {
            // When a player interacts with an enemy, the enemy takes damage.
            TakeDamage(interactor.GetTotalDamage(), interactor);
        }
    }
}