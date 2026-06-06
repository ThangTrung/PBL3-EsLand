using UnityEngine;
using UnityEngine.AI;
using Core.Contracts.AI;

namespace Gameplay.AI.Movement
{
    using Gameplay.Characters;

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovementController : MovementControllerBase
    {
        [Header("AI Specific Settings")]
        [SerializeField] private float movementSmoothing = 12f;
        [SerializeField] private float pathUpdateCooldown = 0.3f;
        [SerializeField] private float destinationChangeThreshold = 0.3f;
        
        private Gameplay.AI.Animation.CharacterAnimationController _animController;
        private float _speedMultiplier = 1f;
        private bool _isNavigating;
        private Vector3 _lastPathDestination = Vector3.positiveInfinity;
        private float _pathCooldownTimer;

        protected override void Awake()
        {
            base.Awake();
            _animController = GetComponentInChildren<Gameplay.AI.Animation.CharacterAnimationController>();
        }

        private void FixedUpdate()
        {
            if (!_canMove || !_isNavigating) return;

            Vector3 destination;
            if (_followTarget != null) destination = _followTarget.position;
            else if (_targetPosition.HasValue) destination = _targetPosition.Value;
            else { _isNavigating = false; return; }

            bool isAtTarget = CheckReachedTarget(destination);

            if (isAtTarget)
            {
                ApplyVelocity(Vector2.zero);
                if (_followTarget == null && _targetPosition.HasValue)
                {
                    CompleteFollow();
                }
                return;
            }

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.nextPosition = transform.position;

                if (Time.time >= _pathCooldownTimer || Vector3.Distance(destination, _lastPathDestination) > destinationChangeThreshold)
                {
                    _agent.SetDestination(destination);
                    _lastPathDestination = destination;
                    _pathCooldownTimer = Time.time + pathUpdateCooldown;
                }

                if (_agent.pathPending) return;

                Vector2 steeringPos = _agent.steeringTarget;
                Vector2 targetDir = (steeringPos - (Vector2)transform.position).normalized;

                if (targetDir.sqrMagnitude > 0.01f)
                {
                    ApplyVelocity(targetDir * GetMoveSpeed());
                }
                else
                {
                    ApplyVelocity(Vector2.zero);
                }
            }
            else
            {
                Vector2 direction = ((Vector2)destination - (Vector2)transform.position).normalized;
                ApplyVelocity(direction * GetMoveSpeed());
            }
        }

        public void Move(Vector3 direction, float speedOverride = -1f)
        {
            if (!_canMove) return;

            if (direction.sqrMagnitude > 0.01f)
            {
                _isNavigating = false;
                if (_agent && _agent.isOnNavMesh) _agent.isStopped = true;
            }

            float speed = speedOverride > 0 ? speedOverride : GetMoveSpeed();
            ApplyVelocity(direction.normalized * speed);
        }

        protected override void UpdateFacing(float directionX)
        {
            if (Time.time < _nextFlipTime) return;
            if (Mathf.Abs(directionX) < minFlippingThreshold) return;

            float targetScaleX = Mathf.Sign(directionX);
            Transform targetTransform = _animController != null ? _animController.transform : transform;
            
            if (!Mathf.Approximately(targetTransform.localScale.x, targetScaleX))
            {
                targetTransform.localScale = new Vector3(targetScaleX, 1, 1);
                _nextFlipTime = Time.time + flipCooldown;
            }
        }

        public void SetTargetPosition(Vector3 position, float stopDistance = 0.5f, System.Action onReached = null)
        {
            _stopDistance = stopDistance;
            _targetPosition = position;
            _followTarget = null;
            _onTargetReached = onReached;
            _canMove = true;
            _isNavigating = true;
            if (_agent && _agent.isOnNavMesh) _agent.isStopped = false;
        }

        public void SetFollowTarget(Transform target, float stopDistance = 1.0f, System.Action onReached = null)
        {
            _stopDistance = stopDistance;
            _followTarget = target;
            _targetPosition = null;
            _onTargetReached = onReached;
            _canMove = true;
            _isNavigating = true;
            if (_agent && _agent.isOnNavMesh) _agent.isStopped = false;
        }

        public override void StopMovement()
        {
            _isNavigating = false;
            base.StopMovement();
        }

        protected override void ApplyVelocity(Vector2 targetVelocity)
        {
            if (!_rb) return;
            _rb.velocity = Vector2.Lerp(_rb.velocity, targetVelocity, Time.fixedDeltaTime * movementSmoothing); 
            UpdateAnimation(_rb.velocity);
            UpdateFacing(_rb.velocity.x);
        }

        protected override float GetMoveSpeed() => Mathf.Max(0.1f, baseMoveSpeed * _speedMultiplier);
        public void SetBaseMoveSpeed(float speed) => baseMoveSpeed = speed;
        public void SetSpeedMultiplier(float multiplier) => _speedMultiplier = multiplier;
    }
}
