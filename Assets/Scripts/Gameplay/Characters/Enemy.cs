using System.Collections.Generic;
using Core.Contracts.Shared;
using Data.Items;
using UnityEngine;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(PlayerMovementController))]
    public class Enemy : Character, IInteractable
    {
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
        [SerializeField] private float baseAttackCooldown = 1f;
        [SerializeField] private float baseDamage = 5f;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float patrolReachDistance = 0.3f;

        [Header("Loot Settings")]
        [SerializeField] private List<Item> lootTable = new List<Item>();
        [Range(0, 1)][SerializeField] private float dropChance = 0.5f;

        private Vector3 _patrolPoint;
        private float _attackTimer;
        
        private CharacterHealth _health;
        private PlayerMovementController _movement;

        public string InteractionAnimationTrigger => "attack";

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _movement = GetComponent<PlayerMovementController>();

            if (target == null && !string.IsNullOrWhiteSpace(targetTag))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go != null) target = go.transform;
            }

            PickNewPatrolPoint();
        }

        private void Start()
        {
            if (_health != null)
            {
                _health.OnDie += HandleDie;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDie -= HandleDie;
            }
        }

        private void Update()
        {
            if (_health != null && _health.IsDead) return;

            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;

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
                    AttemptAttack();
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
            if (_movement == null) return;
            
            var dir = pos - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f) 
            {
                _movement.StopMovement();
                return;
            }

            dir.Normalize();
            _movement.Move(dir);
        }

        private void AttemptAttack()
        {
            if (_attackTimer > 0) return;
            if (!target) return;
            
            if (_movement != null) _movement.StopMovement();

            var distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange) return;

            if (target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var victim))
            {
                victim.TakeDamage(baseDamage, this);
            }

            _attackTimer = baseAttackCooldown;
        }

        private void HandleDie()
        {
            Debug.Log($"[Enemy] {characterName} đã bị tiêu diệt.");
            DropLoot();
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
            // Player InteractionController might pass in Player Facade. We need player damage.
            // For now, PlayerInteractionController passes damage via GetTotalDamage?
            // Actually, PlayerInteractionController has GetTotalDamage. Let's just have PlayerInteractionController directly deal damage,
            // but the original code had interactor.GetTotalDamage(). 
            
            if (interactor != null && interactor.TryGetComponent<PlayerInteractionController>(out var pic))
            {
                _health?.TakeDamage(pic.GetTotalDamage(), interactor);
            }
        }
    }
}
