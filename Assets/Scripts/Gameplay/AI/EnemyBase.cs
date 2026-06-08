using Gameplay.World;
using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.States;
using Gameplay.AI.Strategies;
using Gameplay.AI.Strategies.Modifiers;
using Gameplay.Characters;
using Infrastructure.Pooling;
using Core.Contracts.Shared;
using Gameplay.Spawning;
using UnityEngine.AI;

using UnityEngine;

namespace Gameplay.AI
{
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(EnemyMovementController))]
    public abstract class EnemyBase : Character, IPoolable, IResettable, IInteractable
    {
        private const float DefaultPatrolReachDistance = 0.3f;
        private const float DeathFallbackDelay = 2.5f; // Increased for safety

        protected CharacterHealth HealthInternal;
        protected EnemyMovementController MovementController;
        protected CharacterAnimationController AnimationController;
        protected Gameplay.Combat.StatusEffects.StatusEffectController StatusEffectController;
        private IMovementStrategy _movementStrategy;
        private IDefenseStrategy _defenseStrategy;
        private IAIState _currentState;
        private Transform _target;
        protected bool IsDeadInternal;
        private float _deathStartTime;
        private bool _isSwappingEntities;
        private bool _lootDropped;


        protected IEnemyConfig ConfigInternal;
        protected IAttackStrategy AttackStrategyInternal;
        protected float AttackRangeInternal;

        public Transform Target => _target;
        
        public bool HasValidTarget 
        {
            get 
            {
                if (_target == null) return false;
                if (_target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var damageable))
                {
                    return !damageable.IsDead;
                }
                return true;
            }
        }
        public new CharacterAnimationController Animator => AnimationController;
        
        public IAttackStrategy AttackStrategy => AttackStrategyInternal;
        public IDefenseStrategy DefenseStrategy => _defenseStrategy;
        public IEnemyConfig Config => ConfigInternal;
        public float AttackRange => AttackRangeInternal;
        public float PatrolReachDistance => DefaultPatrolReachDistance;
        
        public Vector3 DebugTargetPosition { get; set; }
        public bool IsDead => IsDeadInternal;

        public virtual IAIState CreateChaseState()
        {
            return new ChaseState();
        }

        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }


        protected virtual void SetInitialState()
        {
            if (_currentState == null)
            {
                ChangeState(new PatrolState());
            }
        }


        public virtual bool CanInteract(Character interactor)
        {
            return !IsDeadInternal;
        }

        public virtual float GetStaminaCost(Character interactor)
        {
            return 5f;
        }

        public virtual void Interact(Character interactor)
        {
            if (IsDeadInternal) return;

            if (interactor != null)
            {
                var pic = interactor.GetComponentInChildren<PlayerInteractionController>();
                if (pic != null)
                {
                    HealthInternal?.TakeDamage(pic.GetTotalDamage(), interactor);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            HealthInternal = GetComponent<CharacterHealth>();
            MovementController = GetComponent<EnemyMovementController>();
            AnimationController = GetComponentInChildren<CharacterAnimationController>();
            StatusEffectController = GetComponent<Gameplay.Combat.StatusEffects.StatusEffectController>();     

            if (HealthInternal != null)
            {
                HealthInternal.OnDie += HandleDie;
                HealthInternal.OnDamageTaken += HandleDamageTaken;
            }

            FindTarget();

            // Global NavMesh Protection
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updatePosition = false;
                agent.updateUpAxis = false;
            }
        }

        protected virtual void Start()
        {
            if (AnimationController != null && AnimationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Idle)
            {
                AnimationController.PlayIdle();
            }

            if (_movementStrategy == null) InitializeMovementStrategy();
            if (_defenseStrategy == null) InitializeDefenseStrategy();

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            SetInitialState();
        }


        private void FindTarget()
        {
            if (_target != null) return;
            _target = Gameplay.Characters.TargetTracker.PlayerTarget;
        }

        private void InitializeMovementStrategy()
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                _movementStrategy = new NavMeshMovementStrategy(MovementController, AnimationController, agent, StatusEffectController);
            }
            else
            {
                _movementStrategy = new SimpleMovementStrategy(MovementController, AnimationController, StatusEffectController);
            }
        }

        protected virtual void InitializeDefenseStrategy()
        {
            if (ConfigInternal == null || ConfigInternal.DefenseChance <= 0) return;
            
            var blockMod = GetComponent<BlockDamageModifier>();
            if (blockMod == null) blockMod = gameObject.AddComponent<BlockDamageModifier>();
            
            _defenseStrategy = new StandardDefenseStrategy(ConfigInternal.DefenseDuration, ConfigInternal.DefenseCooldown, blockMod);
        }

        protected virtual void OnDestroy()
        {
            if (HealthInternal != null)
            {
                HealthInternal.OnDie -= HandleDie;
                HealthInternal.OnDamageTaken -= HandleDamageTaken;
            }
        }

        protected virtual void Update()
        {
            if (IsDeadInternal)
            {
                HandleDeathProcess();
                return;
            }

            if (_target == null) FindTarget();
            
            if (StatusEffectController != null && MovementController != null)
            {
                MovementController.SetSpeedMultiplier(StatusEffectController.SpeedMultiplier);
            }

            _currentState?.Execute(this);
        }

        private void HandleDeathProcess()
        {
            if (AnimationController == null)
            {
                FinishDeath();
                return;
            }

            // [FIX] Ensure we are in Death state before checking if finished
            // This prevents finishing prematurely if the previous state was already finished
            bool isDeathState = AnimationController.GetCurrentState() == AnimationStateNames.Death;
            
            if (isDeathState && AnimationController.IsCurrentAnimationFinished())
            {
                FinishDeath();
            }
            else if (Time.time - _deathStartTime >= DeathFallbackDelay)
            {
                // Safety fallback if animation gets stuck
                FinishDeath();
            }
        }

        private void FinishDeath()
        {
            TriggerLootDrop();
            if (EnemySpawnDirector.Instance != null) EnemySpawnDirector.Instance.UnregisterEnemy(this);
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }

        public virtual void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            if (config != null) ConfigInternal = config;
            if (attackStrategy != null) AttackStrategyInternal = attackStrategy;
            if (attackRange > 0) AttackRangeInternal = attackRange;

            if (animationConfig != null && AnimationController != null) AnimationController.SetConfig(animationConfig);
            if (HealthInternal != null && ConfigInternal != null) HealthInternal.SetMaxHealth(ConfigInternal.MaxHealth, true);

            InitializeMovementStrategy();
            InitializeDefenseStrategy();
            
            SetInitialState();
        }

        public virtual void OnSpawn() { IsDeadInternal = false; }

        public virtual void ResetEnemy()
        {
            _lootDropped = false;
            IsDeadInternal = false;
            _deathStartTime = 0f;
            _isSwappingEntities = false;
            if (HealthInternal != null) HealthInternal.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            
            SetInitialState();
            
            if (StatusEffectController != null) StatusEffectController.ClearAllEffects();
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updatePosition = false;
                agent.updateUpAxis = false;
                if (agent.isOnNavMesh) agent.isStopped = true;
            }

            if (AnimationController != null) { AnimationController.ResetAnimationLock(); AnimationController.PlayIdle(); }
        }

        public virtual void ResetStats() => ResetEnemy();

        public virtual void OnReturn()
        {
            IsDeadInternal = true;
            StopMovement();
        }

        public void ChangeState(IAIState newState)
        {
            if (IsDeadInternal && newState != null) return;
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState?.Enter(this);
        }

        public void MoveTowardsPosition(Vector3 position)
        {
            if (MovementController == null) return;
            MovementController.SetTargetPosition(position, DefaultPatrolReachDistance);
        }

        public void FollowTarget(Transform target, float stopDistance = 1.0f, System.Action onReached = null)
        {
            if (MovementController == null) return;
            MovementController.SetFollowTarget(target, stopDistance, onReached);
        }

        public void StopMovement()
        {
            MovementController?.StopMovement();
        }

        public void FaceTarget()
        {
            if (_target == null || AnimationController == null) return;
            
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.sqrMagnitude < 0.2f)
            {
                var dir = _target.position - transform.position;
                AnimationController.SetFacingDecisive(dir.x);
            }
        }

        public void PrepareForEntitySwap() { _isSwappingEntities = true; }

        private void HandleDie()
        {
            if (IsDeadInternal || _isSwappingEntities) return;
            IsDeadInternal = true;
            _deathStartTime = Time.time;
            
            // [CRITICAL] Stop everything
            StopMovement();
            
            // [FIX] Ensure death animation is rendered at Order 5
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;

            // Disable physics interaction but keep simulation for knockback settling
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            
            // [CRITICAL] Play Death Animation immediately
            if (AnimationController != null)
            {
                AnimationController.PlayDeath();
            }
            
            Core.Events.GameEvents.OnEnemyDied?.Invoke(this);
        }

        private void HandleDamageTaken(float finalDamage, Character source)
        {
            if (IsDeadInternal) return;
            if (_defenseStrategy != null && _currentState != null && _currentState.GetType() != typeof(DefenseState))
            {
                float chance = ConfigInternal != null ? ConfigInternal.DefenseChance : 0.2f;
                if (Random.value <= chance && _defenseStrategy.CanDefend(this))
                {
                    ChangeState(new DefenseState());
                    return;
                }
            }

            if (_currentState != null && _currentState.GetType() != typeof(HitState))
            {
                ChangeState(new HitState());
            }
        }

        private void TriggerLootDrop()
        {
            if (_lootDropped) return;
            _lootDropped = true;

            var spawner = GetComponent<Gameplay.World.LootSpawner>();
            if (spawner != null && ConfigInternal != null)
            {
                spawner.ClearLoot(); // Chuẩn bị nhận đồ mới

                if (ConfigInternal.LootDrops != null)
                {
                    foreach (var drop in ConfigInternal.LootDrops)
                    {
                        if (drop.item != null)
                        {
                            spawner.AddLoot(drop.item, drop.minQuantity, drop.maxQuantity, drop.dropChance);
                        }
                    }
                }

                spawner.SpawnLoot();
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && DebugTargetPosition != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, DebugTargetPosition);
                Gizmos.DrawWireSphere(DebugTargetPosition, 0.2f);
            }
        }
    }
}