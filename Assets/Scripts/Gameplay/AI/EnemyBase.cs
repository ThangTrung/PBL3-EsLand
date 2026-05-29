using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.States;
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
    [RequireComponent(typeof(CharacterAnimationController))]
    public abstract class EnemyBase : Character, IPoolable, IResettable, IInteractable
    {
        private const float DefaultPatrolReachDistance = 0.3f;
        private const float DeathFallbackDelay = 2f;

        private CharacterHealth _health;
        private EnemyMovementController _movementController;
        private CharacterAnimationController _animationController;
        private Gameplay.Combat.StatusEffects.StatusEffectController _statusEffectController;
        private IMovementStrategy _movementStrategy;
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
        public new CharacterAnimationController Animator => _animationController;
        
        public IAttackStrategy AttackStrategy => AttackStrategyInternal;
        public IEnemyConfig Config => ConfigInternal;
        public float AttackRange => AttackRangeInternal;
        public float PatrolReachDistance => DefaultPatrolReachDistance;
        
        public Vector3 DebugTargetPosition { get; set; }
public bool IsDead => IsDeadInternal;

        public virtual IAIState CreateChaseState()
        {
            return new ChaseState();
        }

        public virtual bool CanInteract(Character interactor)
        {
            return !IsDeadInternal;
        }

        public virtual float GetStaminaCost(Character interactor)
        {
            return 5f; // Default combat stamina cost
        }

        public virtual void Interact(Character interactor)
        {
            if (IsDeadInternal) return;

            // When player interacts with an enemy, it counts as an attack
            if (interactor != null)
            {
                var pic = interactor.GetComponentInChildren<PlayerInteractionController>();
                if (pic != null)
                {
                    _health?.TakeDamage(pic.GetTotalDamage(), interactor);
                }
                else
                {
                    Debug.LogWarning($"[EnemyBase] {interactor.name} does not have a PlayerInteractionController in its children.");
                }
            }
        }

protected virtual new void Awake()
        {
            base.Awake();
            _health = GetComponent<CharacterHealth>();
            _movementController = GetComponent<EnemyMovementController>();
            _animationController = GetComponent<CharacterAnimationController>();
            _statusEffectController = GetComponent<Gameplay.Combat.StatusEffects.StatusEffectController>();     

            if (_health != null)
            {
                _health.OnDie += HandleDie;
            }

            FindTarget();

            // [FIX] Global NavMesh Invisibility Protection
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updatePosition = false;
                agent.updateUpAxis = false;
            }

            // [FIX] Fallback for Scene-placed enemies
            if (ConfigInternal == null)
            {
                TryAutoInitialize();
            }
        }

        protected virtual void Start()
        {
            if (_animationController != null && _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Idle)
            {
                _animationController.PlayIdle();
            }

            // [CRITICAL] Fallback initialization for manually placed enemies
            if (_movementStrategy == null)
            {
                InitializeMovementStrategy();
            }

            // [CRITICAL] Ensure NavMeshAgent is on NavMesh
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            if (_currentState == null)
            {
                ChangeState(new PatrolState());
            }
        }

        private void TryAutoInitialize()
        {
            string enemyName = gameObject.name.Replace(" (Clone)", "").Trim();
            var config = Resources.Load<Data.Enemies.SimpleEnemyConfig>($"Enemies/Configs/{enemyName}Config");
            var animConfig = Resources.Load<AnimationConfig>($"Enemies/Animations/{enemyName}Anims");

            if (config != null && animConfig != null)
            {
                InitializeEnemy(config, animConfig, null, config.BaseAttackRange);
                Debug.Log($"[EnemyBase] Auto-initialized {gameObject.name} using Resources.");
            }
        }

        private void FindTarget()
        {
            if (_target != null) return;
            var player = GameObject.FindGameObjectWithTag("Player");
            _target = player != null ? player.transform : null;
        }

        private void InitializeMovementStrategy()
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                _movementStrategy = new NavMeshMovementStrategy(_movementController, _animationController, agent, _statusEffectController);
            }
            else
            {
                _movementStrategy = new SimpleMovementStrategy(_movementController, _animationController, _statusEffectController);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDie -= HandleDie;
            }
        }

        protected virtual void Update()
        {
            if (IsDeadInternal)
            {
                if (_animationController != null && _animationController.IsCurrentAnimationFinished())
                {
                    // PILLAR 2: Kích hoạt hệ thống Loot Event-Driven
                    TriggerLootDrop();

                    // HOTFIX A: Báo cáo cho Director trước khi thu hồi về Pool
                    if (EnemySpawnDirector.Instance != null)
                    {
                        EnemySpawnDirector.Instance.UnregisterEnemy(this);
                    }
                    
                    ObjectPoolManager.Instance.ReturnToPool(gameObject);
                    return;
                }

                if (Time.time - _deathStartTime >= DeathFallbackDelay)
                {
                    // PILLAR 2: Fallback loot drop
                    TriggerLootDrop();

                    // HOTFIX A: Fallback check
                    if (EnemySpawnDirector.Instance != null)
                    {
                        EnemySpawnDirector.Instance.UnregisterEnemy(this);
                    }

                    ObjectPoolManager.Instance.ReturnToPool(gameObject);
                }

                return;
            }

            // Ensure target is still valid
            if (_target == null) FindTarget();

            _currentState?.Execute(this);
        }

        public virtual void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            if (config != null) ConfigInternal = config;
            if (attackStrategy != null) AttackStrategyInternal = attackStrategy;
            if (attackRange > 0) AttackRangeInternal = attackRange;

            if (animationConfig != null && _animationController != null)
            {
                _animationController.SetConfig(animationConfig);
            }

            if (_health != null && ConfigInternal != null)
            {
                _health.SetMaxHealth(ConfigInternal.MaxHealth, true);
            }

            InitializeMovementStrategy();
            ChangeState(new PatrolState());
        }

        public virtual void OnSpawn()
        {
            IsDeadInternal = false;
        }

        public virtual void ResetEnemy()
        {
            _lootDropped = false;

            IsDeadInternal = false;
            _deathStartTime = 0f;
            _isSwappingEntities = false;
            
            // 1. Hồi 100% máu
            if (_health != null)
            {
                _health.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            }

            // 2. Đưa StateMachine về trạng thái mặc định
            ChangeState(new PatrolState());

            // 3. Xóa mọi Status Effects
            if (_statusEffectController != null)
            {
                _statusEffectController.ClearAllEffects();
            }

            // 4. Đảm bảo Collider/SpriteRenderer được bật lại
            var sr = GetComponent<SpriteRenderer>();
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

            if (_animationController != null)
            {
                _animationController.PlayIdle();
            }
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
            if (_movementStrategy == null) return;
            _movementStrategy.Move(position);
        }

        public void StopMovement()
        {
            _movementStrategy?.StopMovement();
        }

        public void FaceTarget()
        {
            if (_target == null || _animationController == null) return;
            var dir = _target.position - transform.position;
            
            // Decisively face the target regardless of movement deadzone
            _animationController.SetFacingDecisive(dir.x);
        }

        public void PrepareForEntitySwap()
        {
            _isSwappingEntities = true;
        }

        private void HandleDie()
        {
            if (IsDeadInternal || _isSwappingEntities) return;
            IsDeadInternal = true;
            _deathStartTime = Time.time;
            _movementStrategy?.StopMovement();
            _animationController?.PlayDeath();
            
            Core.Events.GameEvents.OnEnemyDied?.Invoke(this);
        }

        private void TriggerLootDrop()
        {
            if (_lootDropped) return;
            _lootDropped = true;

            if (ConfigInternal != null && !string.IsNullOrEmpty(ConfigInternal.LootItemId))
            {
                var lootData = new Data.Loot.LootDropData(
                    ConfigInternal.LootItemId,
                    ConfigInternal.LootQuantity,
                    transform.position
                );

                Core.Events.GameEvents.InvokeEnemyDroppedLoot(lootData);
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