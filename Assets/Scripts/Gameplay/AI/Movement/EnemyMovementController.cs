using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.AI.Movement
{
    /// <summary>
    /// Handles physical movement for enemy entities.
    /// Unified controller for NavMesh and Physics.
    /// Optimized for reliability: AI will never get stuck or 'forget' to chase.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 3f;
        [SerializeField] private float movementSmoothing = 12f; 
        private float _speedMultiplier = 1f;

        private Rigidbody2D _rb;
        private Gameplay.Characters.Character _facade;
        private Gameplay.AI.Animation.CharacterAnimationController _animController;
        private Collider2D _myCollider;
        private NavMeshAgent _agent;

        private bool _canMove = true;
        private Transform _followTarget;
        private Vector3? _targetPosition;
        private float _stopDistance = 0.5f;
        private System.Action _onTargetReached;

        // --- Navigation State ---
        private bool _isNavigating;
        private Vector3 _lastPathDestination = Vector3.positiveInfinity;
        private float _pathCooldownTimer;
        [SerializeField] private float pathUpdateCooldown = 0.3f; 
        [SerializeField] private float destinationChangeThreshold = 0.3f;
        
        private float _lastFlipTime;
        private const float FlipCooldown = 0.2f;
        private const float MinFlippingThreshold = 0.15f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Gameplay.Characters.Character>();
            _animController = GetComponent<Gameplay.AI.Animation.CharacterAnimationController>();
            _myCollider = GetComponent<Collider2D>();
            _agent = GetComponent<NavMeshAgent>();

            if (_agent == null) return;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        private void Start()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie += HandleDie;

            EnsureAgentOnNavMesh();
        }

        private void OnDestroy()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie -= HandleDie;
        }

        private void FixedUpdate()
        {
            if (!_canMove || !_isNavigating) return;

            Vector3 destination;
            if (_followTarget != null) destination = _followTarget.position;
            else if (_targetPosition.HasValue) destination = _targetPosition.Value;
            else { _isNavigating = false; return; }

            // [FIX] KIỂM TRA ĐÍCH ĐẾN: Nếu đã tới tầm, chỉ dừng lại nhưng VẪN giữ trạng thái Navigating
            bool isAtTarget = CheckReachedTarget(destination);
            
            if (isAtTarget)
            {
                ApplyVelocity(Vector2.zero);
                
                if (!_followTarget.HasValue() && _targetPosition.HasValue)
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
                    UpdateFacing(targetDir.x);
                    ApplyVelocity(targetDir * GetCurrentMoveSpeed());
                }
                else
                {
                    ApplyVelocity(Vector2.zero);
                }
            }
            else
            {
                Vector2 direction = ((Vector2)destination - (Vector2)transform.position).normalized;
                UpdateFacing(direction.x);
                ApplyVelocity(direction * GetCurrentMoveSpeed());
            }
        }

        public void Move(Vector3 direction, float speedOverride = -1f)
        {
            if (!_canMove) return;

            if (direction.sqrMagnitude > 0.01f)
            {
                _isNavigating = false;
                if (_agent && _agent.isOnNavMesh) _agent.isStopped = true;
                UpdateFacing(direction.x);
            }

            float speed = speedOverride > 0 ? speedOverride : GetCurrentMoveSpeed();
            ApplyVelocity(direction.normalized * speed);
        }

        private void UpdateFacing(float directionX)
        {
            if (Mathf.Abs(directionX) > MinFlippingThreshold && Time.time >= _lastFlipTime + FlipCooldown)
            {
                float targetScaleX = Mathf.Sign(directionX);
                if (!Mathf.Approximately(transform.localScale.x, targetScaleX))
                {
                    transform.localScale = new Vector3(targetScaleX, 1, 1);
                    _lastFlipTime = Time.time;
                }
            }
        }

        public void SetTargetPosition(Vector3 position, float stopDistance = 0.5f, System.Action onReached = null)
        {
            // [FIX] Luôn cập nhật stopDistance mới kể cả khi đích đến không đổi
            _stopDistance = stopDistance;

            if (_isNavigating && _targetPosition.HasValue && Vector3.Distance(_targetPosition.Value, position) < 0.1f) return;

            _targetPosition = position;
            _followTarget = null;
            _onTargetReached = onReached;
            _canMove = true;
            _isNavigating = true;

            if (_agent && _agent.isOnNavMesh) _agent.isStopped = false;
        }

        public void SetFollowTarget(Transform target, float stopDistance = 1.0f, System.Action onReached = null)
        {
            // [FIX] Luôn cập nhật stopDistance mới
            _stopDistance = stopDistance;

            if (_isNavigating && _followTarget == target) return;

            _followTarget = target;
            _targetPosition = null;
            _onTargetReached = onReached;
            _canMove = true;
            _isNavigating = true;

            if (_agent && _agent.isOnNavMesh) _agent.isStopped = false;
        }

        public void StopMovement()
        {
            _isNavigating = false;
            _followTarget = null;
            _targetPosition = null;
            _onTargetReached = null;
            if (_agent && _agent.isOnNavMesh) _agent.isStopped = true;
            ApplyVelocity(Vector2.zero);
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove) StopMovement();
        }

        private void CompleteFollow()
        {
            var callback = _onTargetReached;
            StopMovement();
            callback?.Invoke();
        }

        private void ApplyVelocity(Vector2 targetVelocity)
        {
            if (!_rb) return;
            
            _rb.velocity = Vector2.Lerp(_rb.velocity, targetVelocity, Time.fixedDeltaTime * movementSmoothing);
            
            bool isMoving = _rb.velocity.sqrMagnitude > 0.1f;
            
            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetBool(IsMovingHash, isMoving);

            if (_animController != null)
            {
                if (isMoving) _animController.PlayRun();
                else _animController.PlayIdle();
            }
        }

        private bool CheckReachedTarget(Vector3 destination)
        {
            if (_followTarget != null)
            {
                var targetCollider = _followTarget.GetComponent<Collider2D>();
                if (targetCollider && _myCollider)
                {
                    if (_myCollider.IsTouching(targetCollider)) return true;
                    var result = Physics2D.Distance(_myCollider, targetCollider);
                    if (result.distance <= _stopDistance) return true;
                }
            }
            
            float dist = Vector2.Distance(transform.position, destination);
            if (dist <= _stopDistance) return true;

            if (_agent && _agent.enabled && _agent.isOnNavMesh && !_agent.pathPending)
            {
                if (_agent.remainingDistance <= 0.2f && dist <= _stopDistance + 0.5f) return true;
            }

            return false;
        }

        private void EnsureAgentOnNavMesh()
        {
            if (!_agent) return;
            if (_agent.isOnNavMesh) return;
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
            if (_agent && _agent.isOnNavMesh) 
                _agent.isStopped = true;
        }

        public void SetBaseMoveSpeed(float speed) => baseMoveSpeed = speed;
        public void SetSpeedMultiplier(float multiplier) => _speedMultiplier = multiplier;
        public float GetCurrentMoveSpeed() => Mathf.Max(0.1f, baseMoveSpeed * _speedMultiplier);
    }

    public static class TransformExtensions
    {
        public static bool HasValue(this Transform t) => t != null;
    }
}