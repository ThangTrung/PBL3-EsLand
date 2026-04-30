using System.Collections.Generic;
using Script.Items;
using Script.Shared.Interfaces;
using UnityEngine;

namespace Script.Entities
{
    /// <summary>
    /// Enemy kế thừa Character: dùng stat/combat/cooldown từ Character,
    /// AI chỉ quyết định hành vi (patrol/chase/attack).
    /// </summary>
    public class Enemy : Character, IInteractable, IInventoryHolder
    {
        public override Script.Inventory.Interfaces.IInventory Inventory => GetComponentInChildren<Script.Inventory.Interfaces.IInventory>();

        private enum AIState
        {
            Patrol,
            Chase,
            Attack
        }

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private string targetTag = "Player";

        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private AIState state = AIState.Patrol;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float patrolReachDistance = 0.3f;

        [Header("Loot Settings (temporary)")]
        [SerializeField] private List<Item> lootTable = new List<Item>();
        [Range(0, 1)][SerializeField] private float dropChance = 0.5f;

        private Vector3 _patrolPoint;
        
        // Implementation of the new interface property
        public string InteractionAnimationTrigger => "attack";

        protected override void Awake()
        {
            base.Awake();
            
            if (target == null && !string.IsNullOrWhiteSpace(targetTag))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go != null) target = go.transform;
            }

            PickNewPatrolPoint();
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            UpdateState();
            TickState();
        }

        private void UpdateState()
        {
            if (!target)
            {
                state = AIState.Patrol;
                return;
            }

            var distance = Vector3.Distance(transform.position, target.position);

            if (distance <= attackRange) state = AIState.Attack;
            else if (distance <= detectionRange) state = AIState.Chase;
            else state = AIState.Patrol;
        }

        private void TickState()
        {
            switch (state)
            {
                case AIState.Patrol:
                    Patrol();
                    break;

                case AIState.Chase:
                    ChaseTarget();
                    break;

                case AIState.Attack:
                    Attack(); // dùng Attack() chuẩn của Character (cooldown)
                    break;
            }
        }

        private void Patrol()
        {
            MoveTowards(_patrolPoint);

            if (Vector3.Distance(transform.position, _patrolPoint) <= patrolReachDistance)
                PickNewPatrolPoint();
        }

        private void PickNewPatrolPoint()
        {
            var rand = Random.insideUnitSphere * patrolRadius;
            rand.y = 0f;
            _patrolPoint = transform.position + rand;
        }

        private void ChaseTarget()
        {
            if (!target) return;
            MoveTowards(target.position);
        }

        private void MoveTowards(Vector3 pos)
        {
            var dir = pos - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f) return;

            dir.Normalize();

            // Sẽ dùng phương thức Move kế thừa từ Character thay vì tự tính transform.position
            Move(dir);

            FaceDirection(dir);
        }

        private static void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            // Character.Move đã xử lý lật hình qua localScale dựa trên direction.x
            // Nếu game là Top-down thật sự cần transform.forward thì giữ lại
            // Ở Character.cs: transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }

        public override void Attack()
        {
            if (!CanAttack()) return;
            if (!target) return;
            
            var distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange) return;

            if (target.TryGetComponent<IDamageable>(out var victim))
                victim.TakeDamage(GetTotalDamage(), this);

            AttackTimer = baseAttackCooldown;
        }

        protected override void Die()
        {
            base.Die();

            Debug.Log($"[Enemy] {characterName} đã bị tiêu diệt.");

            DropLoot();

            // tuỳ game: có thể disable thay vì destroy (để object pool)
            Destroy(gameObject, 2f);
        }

        private void DropLoot()
        {
            if (lootTable == null || lootTable.Count == 0) return;

            if (Random.value > dropChance) return;

            int idx = Random.Range(0, lootTable.Count);
            var dropped = lootTable[idx];
            if (dropped == null) return;

            Debug.Log($"[Loot] {characterName} rơi ra vật phẩm: {dropped.ItemName}");

            // TODO: Instantiate pickup prefab (vật phẩm rơi ra ngoài world)
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }

        public void Interact(Character interactor)
        {
            // When a player interacts with an enemy, the enemy takes damage.
            TakeDamage(interactor.GetTotalDamage(), interactor);
        }
    }
}