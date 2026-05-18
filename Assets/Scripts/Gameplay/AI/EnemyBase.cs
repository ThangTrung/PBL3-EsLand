using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.States;
using Gameplay.Characters;
using Infrastructure.Pooling;
using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.AI
{
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(CharacterAnimationController))]
    public abstract class EnemyBase : Character, IPoolable, IResettable
    {
        private const float DefaultPatrolReachDistance = 0.3f;
        private const float DeathFallbackDelay = 2f;

        private CharacterHealth _health;
        private PlayerMovementController _movementController;
        private CharacterAnimationController _animationController;
        private Gameplay.Combat.StatusEffects.StatusEffectController _statusEffectController;
        private IMovementStrategy _movementStrategy;
        private IAIState _currentState;
        private Transform _target;
        protected bool IsDeadInternal;
        private float _deathStartTime;
        private bool _isSwappingEntities;

        protected IEnemyConfig ConfigInternal;
        protected IAttackStrategy AttackStrategyInternal;
        protected float AttackRangeInternal;

        public Transform Target => _target;
        public CharacterAnimationController Animator => _animationController;
        public IAttackStrategy AttackStrategy => AttackStrategyInternal;
        public IEnemyConfig Config => ConfigInternal;
        public float AttackRange => AttackRangeInternal;
        public float PatrolReachDistance => DefaultPatrolReachDistance;
        public bool IsDead => IsDeadInternal;

        public virtual IAIState CreateChaseState()
        {
            return new ChaseState();
        }

        protected virtual void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _movementController = GetComponent<PlayerMovementController>();
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
            if (_animationController != null && _animationController.GetCurrentState() != CharacterAnimationController.AnimState.Idle)
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
                    ObjectPoolManager.Instance.Return(gameObject);
                    return;
                }

                if (Time.time - _deathStartTime >= DeathFallbackDelay)
                {
                    ObjectPoolManager.Instance.Return(gameObject);
                }

                return;
            }

            _currentState?.Execute(this);
        }

        public virtual void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            ConfigInternal = config;
            AttackStrategyInternal = attackStrategy;
            AttackRangeInternal = attackRange;

            if (_animationController != null)
            {
                _animationController.SetConfig(animationConfig);
            }

            if (_health != null)
            {
                _health.SetMaxHealth(config.MaxHealth, true);
            }

            _movementStrategy = new SimpleMovementStrategy(_movementController, _animationController, _statusEffectController);
            ChangeState(new PatrolState());
        }

        public virtual void OnSpawn()
        {
            IsDeadInternal = false;
        }

        public virtual void ResetStats()
        {
            IsDeadInternal = false;
            _deathStartTime = 0f;
            _isSwappingEntities = false;
            
            if (_health != null)
            {
                // Reset health to max based on current config
                _health.SetMaxHealth(ConfigInternal?.MaxHealth ?? 100f, true);
            }

            if (_animationController != null)
            {
                _animationController.PlayIdle();
            }

            if (_statusEffectController != null)
            {
                _statusEffectController.ClearAllEffects();
            }
        }

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
        }
    }
}
