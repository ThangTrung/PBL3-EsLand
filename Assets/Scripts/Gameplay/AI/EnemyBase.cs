using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.States;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI
{
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(CharacterAnimationController))]
    public abstract class EnemyBase : Character
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
        private bool _isDead;
        private float _deathStartTime;

        protected IEnemyConfig ConfigInternal;
        protected IAttackStrategy AttackStrategyInternal;
        protected float AttackRangeInternal;

        public Transform Target => _target;
        public CharacterAnimationController Animator => _animationController;
        public IAttackStrategy AttackStrategy => AttackStrategyInternal;
        public IEnemyConfig Config => ConfigInternal;
        public float AttackRange => AttackRangeInternal;
        public float PatrolReachDistance => DefaultPatrolReachDistance;

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
            if (_isDead)
            {
                if (_animationController != null && _animationController.IsCurrentAnimationFinished())
                {
                    Destroy(gameObject);
                    return;
                }

                if (Time.time - _deathStartTime >= DeathFallbackDelay)
                {
                    Destroy(gameObject);
                }

                return;
            }

            _currentState?.Execute(this);
        }

        protected void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            ConfigInternal = config;
            AttackStrategyInternal = attackStrategy;
            AttackRangeInternal = attackRange;

            if (_animationController != null)
            {
                _animationController.SetConfig(animationConfig);
            }

            _movementStrategy = new SimpleMovementStrategy(_movementController, _animationController, _statusEffectController);
            ChangeState(new PatrolState());
        }

        public void ChangeState(IAIState newState)
        {
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

        private void HandleDie()
        {
            if (_isDead) return;
            _isDead = true;
            _deathStartTime = Time.time;
            _movementStrategy?.StopMovement();
            _animationController?.PlayDeath();
        }
    }
}
