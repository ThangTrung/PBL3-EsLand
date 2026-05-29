using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.States;
using Gameplay.Characters;
using Infrastructure.Pooling;
using Core.Contracts.Shared;
using Gameplay.Spawning;

using UnityEngine;

namespace Gameplay.AI
{
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(EnemyMovementController))]
    [RequireComponent(typeof(CharacterAnimationController))]
    public abstract class EnemyBase : Character, IPoolable, IResettable
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

            var player = GameObject.FindGameObjectWithTag("Player");
            _target = player != null ? player.transform : null;
        }

        protected virtual void Start()
        {
            if (_animationController != null && _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Idle)
            {
                _animationController.PlayIdle();
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
                    // PILLAR 2: KÃ­ch hoáº¡t há»‡ thá»‘ng Loot Event-Driven
                    TriggerLootDrop();

                    // HOTFIX A: BÃ¡o cÃ¡o cho Director trÆ°á»›c khi thu há»“i vá» Pool
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

            _movementStrategy = new SimpleMovementStrategy(_movementController, _animationController, _statusEffectController);
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
            
            // 1. Há»“i 100% mÃ¡u
            if (_health != null)
            {
                _health.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            }

            // 2. ÄÆ°a StateMachine vá» tráº¡ng thÃ¡i máº·c Ä‘á»‹nh
            ChangeState(new PatrolState());

            // 3. XÃ³a má»i Status Effects
            if (_statusEffectController != null)
            {
                _statusEffectController.ClearAllEffects();
            }

            // 4. Äáº£m báº£o Collider/SpriteRenderer Ä‘Æ°á»£c báº­t láº¡i
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
            
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

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

            var direction = position - transform.position;
            direction.z = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                _movementStrategy.StopMovement();
                return;
            }

            direction.Normalize();
            _movementStrategy.Move(direction);
        }

        public void StopMovement()
        {
            _movementStrategy?.StopMovement();
        }

        public void FaceTarget()
        {
            if (_target == null || _animationController == null) return;
            var dir = _target.position - transform.position;
            _animationController.SetFacingByMove(dir);
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
