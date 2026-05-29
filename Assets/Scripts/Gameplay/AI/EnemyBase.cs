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
    [RequireComponent(typeof(CharacterAnimationController))]
    public abstract class EnemyBase : Character, IPoolable, IResettable, IInteractable
    {
        private const float DefaultPatrolReachDistance = 0.3f;
        private const float DeathFallbackDelay = 2f;

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
            AnimationController = GetComponent<CharacterAnimationController>();
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

            if (ConfigInternal == null)
            {
                TryAutoInitialize();
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

            if (_currentState == null)
            {
                ChangeState(new PatrolState());
            }
        }

        private void TryAutoInitialize()
        {
            string enemyName = gameObject.name.Replace(" (Clone)", "").Trim();
            var configAsset = Resources.Load($"Enemies/Configs/{enemyName}Config");
            var animConfig = Resources.Load<AnimationConfig>($"Enemies/Animations/{enemyName}Anims");

            if (configAsset != null && animConfig != null)
            {
                IEnemyConfig config = configAsset as IEnemyConfig;
                if (config != null)
                {
                    InitializeEnemy(config, animConfig, null, config.BaseAttackRange);
                }
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
                _movementStrategy = new NavMeshMovementStrategy(MovementController, AnimationController, agent, StatusEffectController);
            }
            else
            {
                _movementStrategy = new SimpleMovementStrategy(MovementController, AnimationController, StatusEffectController);
            }
        }

        protected virtual void InitializeDefenseStrategy()
        {
            if (ConfigInternal == null) return;
            var blockMod = GetComponent<BlockDamageModifier>();
            if (blockMod == null) blockMod = gameObject.AddComponent<BlockDamageModifier>();
            
            string enemyName = gameObject.name.Replace(" (Clone)", "").Trim();
            if (enemyName.Contains("Turtle"))
                _defenseStrategy = new TurtleDefenseStrategy(ConfigInternal.DefenseDuration, ConfigInternal.DefenseCooldown, blockMod);
            else if (enemyName.Contains("Minotaur") || enemyName.Contains("Skull"))
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
                if (AnimationController != null && AnimationController.IsCurrentAnimationFinished())
                {
                    TriggerLootDrop();
                    if (EnemySpawnDirector.Instance != null) EnemySpawnDirector.Instance.UnregisterEnemy(this);
                    ObjectPoolManager.Instance.ReturnToPool(gameObject);
                    return;
                }

                if (Time.time - _deathStartTime >= DeathFallbackDelay)
                {
                    TriggerLootDrop();
                    if (EnemySpawnDirector.Instance != null) EnemySpawnDirector.Instance.UnregisterEnemy(this);
                    ObjectPoolManager.Instance.ReturnToPool(gameObject);
                }
                return;
            }

            if (_target == null) FindTarget();
            _currentState?.Execute(this);
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
            
            if (_currentState == null) ChangeState(new PatrolState());
        }

        public virtual void OnSpawn() { IsDeadInternal = false; }

        public virtual void ResetEnemy()
        {
            _lootDropped = false;
            IsDeadInternal = false;
            _deathStartTime = 0f;
            _isSwappingEntities = false;
            if (HealthInternal != null) HealthInternal.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            ChangeState(new PatrolState());
            if (StatusEffectController != null) StatusEffectController.ClearAllEffects();
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

            if (AnimationController != null) AnimationController.PlayIdle();
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
            if (_target == null || AnimationController == null) return;
            var dir = _target.position - transform.position;
            AnimationController.SetFacingDecisive(dir.x);
        }

        public void PrepareForEntitySwap() { _isSwappingEntities = true; }

        private void HandleDie()
        {
            if (IsDeadInternal || _isSwappingEntities) return;
            IsDeadInternal = true;
            _deathStartTime = Time.time;
            _movementStrategy?.StopMovement();
            AnimationController?.PlayDeath();
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
                }
            }
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