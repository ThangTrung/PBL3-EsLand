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
    /// <summary>
    /// Base class for all enemies. Handles AI state machine and direct movement control.
    /// Simplified to use EnemyMovementController directly, mirroring Player architecture.
    /// </summary>
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

        public virtual IAIState CreateChaseState() => new ChaseState();

        public virtual bool CanInteract(Character interactor) => !IsDeadInternal;

        public virtual float GetStaminaCost(Character interactor) => 5f;

        public virtual void Interact(Character interactor)
        {
            if (IsDeadInternal || interactor == null) return;

            var pic = interactor.GetComponentInChildren<PlayerInteractionController>();
            if (pic != null)
            {
                _health?.TakeDamage(pic.GetTotalDamage(), interactor);
            }
        }

        protected virtual new void Awake()
        {
            base.Awake();
            _health = GetComponent<CharacterHealth>();
            _movementController = GetComponent<EnemyMovementController>();
            _animationController = GetComponent<CharacterAnimationController>();
            _statusEffectController = GetComponent<Gameplay.Combat.StatusEffects.StatusEffectController>();     

            if (_health != null) _health.OnDie += HandleDie;

            FindTarget();

            if (ConfigInternal == null) TryAutoInitialize();
        }

        protected virtual void Start()
        {
            if (_animationController != null) _animationController.PlayIdle();

            if (_currentState == null) ChangeState(new PatrolState());
        }

        private void TryAutoInitialize()
        {
            string enemyName = gameObject.name.Replace(" (Clone)", "").Trim();
            var config = Resources.Load<Data.Enemies.SimpleEnemyConfig>($"Enemies/Configs/{enemyName}Config");
            var animConfig = Resources.Load<AnimationConfig>($"Enemies/Animations/{enemyName}Anims");

            if (config != null && animConfig != null)
            {
                InitializeEnemy(config, animConfig, null, config.BaseAttackRange);
            }
        }

        private void FindTarget()
        {
            if (_target != null) return;
            var player = GameObject.FindGameObjectWithTag("Player");
            _target = player != null ? player.transform : null;
        }

        protected virtual void OnDestroy()
        {
            if (_health != null) _health.OnDie -= HandleDie;
        }

        protected virtual void Update()
        {
            if (IsDeadInternal)
            {
                HandleDeathProcess();
                return;
            }

            if (_target == null) FindTarget();

            // Sync speed multiplier from status effects
            if (_statusEffectController != null && _movementController != null)
            {
                _movementController.SetSpeedMultiplier(_statusEffectController.SpeedMultiplier);
            }

            _currentState?.Execute(this);
        }

        private void HandleDeathProcess()
        {
            if (_animationController != null && _animationController.IsCurrentAnimationFinished() || 
                (Time.time - _deathStartTime >= DeathFallbackDelay))
            {
                TriggerLootDrop();
                if (EnemySpawnDirector.Instance != null) EnemySpawnDirector.Instance.UnregisterEnemy(this);
                ObjectPoolManager.Instance.ReturnToPool(gameObject);
            }
        }

        public virtual void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            if (config != null) ConfigInternal = config;
            if (attackStrategy != null) AttackStrategyInternal = attackStrategy;
            if (attackRange > 0) AttackRangeInternal = attackRange;

            if (animationConfig != null && _animationController != null) _animationController.SetConfig(animationConfig);

            if (_health != null && ConfigInternal != null)
            {
                _health.SetMaxHealth(ConfigInternal.MaxHealth, true);
            }

            ChangeState(new PatrolState());
        }

        public virtual void OnSpawn() => IsDeadInternal = false;

        public virtual void ResetEnemy()
        {
            _lootDropped = false;
            IsDeadInternal = false;
            _deathStartTime = 0f;
            _isSwappingEntities = false;
            
            if (_health != null) _health.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            if (_statusEffectController != null) _statusEffectController.ClearAllEffects();

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            ChangeState(new PatrolState());
            if (_animationController != null) _animationController.PlayIdle();
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
            if (_movementController == null) return;
            // Uses NavMesh pathfinding
            _movementController.SetTargetPosition(position, DefaultPatrolReachDistance);
        }

        public void FollowTarget(Transform target, float stopDistance = 1.0f, System.Action onReached = null)
        {
            if (_movementController == null) return;
            // Uses NavMesh pathfinding + dynamic update
            _movementController.SetFollowTarget(target, stopDistance, onReached);
        }

        public void StopMovement()
        {
            _movementController?.StopMovement();
        }

        public void FaceTarget()
        {
            if (_target == null || _animationController == null) return;
            
            // CHỈ XOAY MẶT KHI ĐỨNG YÊN HOẶC ĐANG TẤN CÔNG (Vận tốc thấp)
            // Nếu đang chạy, để EnemyMovementController tự lật hình theo hướng chạy
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.sqrMagnitude < 0.2f)
            {
                var dir = _target.position - transform.position;
                _animationController.SetFacingDecisive(dir.x);
            }
        }

        public void PrepareForEntitySwap() => _isSwappingEntities = true;

        private void HandleDie()
        {
            if (IsDeadInternal || _isSwappingEntities) return;
            IsDeadInternal = true;
            _deathStartTime = Time.time;
            StopMovement();
            _animationController?.PlayDeath();
            Core.Events.GameEvents.OnEnemyDied?.Invoke(this);
        }

        private void TriggerLootDrop()
        {
            if (_lootDropped) return;
            _lootDropped = true;

            if (ConfigInternal != null && !string.IsNullOrEmpty(ConfigInternal.LootItemId))
            {
                var lootData = new Data.Loot.LootDropData(ConfigInternal.LootItemId, ConfigInternal.LootQuantity, transform.position);
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