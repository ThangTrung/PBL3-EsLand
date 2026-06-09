using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.AI.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 3f;
        [SerializeField] private float movementSmoothing = 12f;
        [SerializeField] private float steeringSmoothing = 10f; // New: Smoother turns
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

        private bool _isNavigating;
        private Vector3 _lastPathDestination = Vector3.positiveInfinity;
        private float _pathCooldownTimer;
        [SerializeField] private float pathUpdateCooldown = 0.3f;
        [SerializeField] private float destinationChangeThreshold = 0.3f;

        private Vector2 _currentSteeringDir; // New: To track smoothed steering

        private float _lastFlipTime;
        private const float FlipCooldown = 0.2f;
        private const float MinFlippingThreshold = 0.15f;

        private float _navMeshRetryTimer;
        private const float NavMeshRetryInterval = 1.0f;

        public bool IsNavigating => _isNavigating;
        public float StopDistance => _stopDistance;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Gameplay.Characters.Character>();
            _animController = GetComponentInChildren<Gameplay.AI.Animation.CharacterAnimationController>();
            _myCollider = GetComponent<Collider2D>();
            _agent = GetComponent<NavMeshAgent>();

            if (_rb != null)
            {
                // [Senior Fix] Bật Interpolate để mượt hóa chuyển động giữa các frame
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            if (_agent == null) return;
            // [Senior Fix] Đảm bảo cấu hình NavMeshAgent 2D chuẩn xác
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false; 
        }

        private void Start()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie += HandleDie;

            if (GetCurrentMoveSpeed() <= 0.11f)
            {
                Debug.LogWarning($"[EnemyMovementController] {gameObject.name} has very low move speed ({GetCurrentMoveSpeed()}). Check Config!");
            }

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

            // [CRITICAL FIX] NavMeshAgent Authority Conflict
            if (_agent != null)
            {
                if (!_agent.isOnNavMesh)
                {
                    if (_agent.enabled) _agent.enabled = false;
                    
                    _navMeshRetryTimer += Time.fixedDeltaTime;
                    if (_navMeshRetryTimer >= NavMeshRetryInterval)
                    {
                        _navMeshRetryTimer = 0f;
                        EnsureAgentOnNavMesh();
                    }
                }
                else
                {
                    if (!_agent.enabled) _agent.enabled = true;
                    _agent.updatePosition = false;
                    _agent.updateRotation = false;
                    
                    // [Senior Fix] Only sync agent position if it's too far from the physical transform
                    // to prevent micro-jitter while keeping the pathing relevant
                    float distFromAgent = Vector3.Distance(_agent.nextPosition, transform.position);
                    if (distFromAgent > 0.1f)
                    {
                        _agent.nextPosition = transform.position;
                    }
                }
            }

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

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                if (Time.time >= _pathCooldownTimer || Vector3.Distance(destination, _lastPathDestination) > destinationChangeThreshold)
                {
                    _agent.SetDestination(destination);
                    _lastPathDestination = destination;
                    _pathCooldownTimer = Time.time + pathUpdateCooldown;
                }

                Vector2 targetDir;
                
                // [SENIOR FIX] Don't stop moving if path is pending. Use last valid direction or direct fallback.
                if (_agent.pathPending)
                {
                    targetDir = ((Vector2)destination - (Vector2)transform.position).normalized;
                }
                else
                {
                    targetDir = _agent.desiredVelocity.normalized;
                    
                    // If desired velocity is near zero but we haven't reached target yet (e.g. cornering)
                    // Use a direct direction to keep the momentum
                    if (targetDir.sqrMagnitude < 0.01f && !isAtTarget)
                    {
                        targetDir = ((Vector2)destination - (Vector2)transform.position).normalized;
                    }
                }

                // [SENIOR OPTIMIZATION] Steering Smoothing
                // Blend the current steering direction towards the target direction to smooth out cornering
                if (targetDir.sqrMagnitude > 0.01f)
                {
                    if (_currentSteeringDir.sqrMagnitude < 0.01f) _currentSteeringDir = targetDir;
                    _currentSteeringDir = Vector2.Lerp(_currentSteeringDir, targetDir, Time.fixedDeltaTime * steeringSmoothing);
                    
                    UpdateFacing(_currentSteeringDir.x);
                    ApplyVelocity(_currentSteeringDir * GetCurrentMoveSpeed());
                    return;
                }
            }
            
            // [Fallback]
            Vector2 fallbackDir = ((Vector2)destination - (Vector2)transform.position).normalized;
            _currentSteeringDir = Vector2.Lerp(_currentSteeringDir, fallbackDir, Time.fixedDeltaTime * steeringSmoothing);
            
            UpdateFacing(_currentSteeringDir.x);
            ApplyVelocity(_currentSteeringDir * GetCurrentMoveSpeed());
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
                Transform targetTransform = _animController != null ? _animController.transform : transform;
                if (!Mathf.Approximately(targetTransform.localScale.x, targetScaleX))
                {
                    targetTransform.localScale = new Vector3(targetScaleX, 1, 1);
                    _lastFlipTime = Time.time;
                }
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
            if (!canMove) 
            {
                StopMovement();
            }
            else
            {
                _isNavigating = false; 
            }
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

            // [Critical Fix] Đảm bảo Rigidbody luôn thức tỉnh
            if (_rb.IsSleeping()) _rb.WakeUp();

            // Vô hiệu hóa Root Motion để tránh xung đột vị trí
            if (_animController != null && _animController.GetComponent<Animator>() != null)
            {
                _animController.GetComponent<Animator>().applyRootMotion = false;
            }

            // [SENIOR OPTIMIZATION] Use a more robust smoothing that accounts for delta time properly
            // and uses dynamic acceleration to avoid jerky starts/stops.
            Vector2 currentVelocity = _rb.velocity;
            
            // Calculate a smoother acceleration factor
            float acceleration = GetCurrentMoveSpeed() * movementSmoothing;
            
            // For braking or sharp turns, we can adjust the acceleration
            if (targetVelocity.sqrMagnitude < 0.01f) acceleration *= 1.5f; // Faster braking
            
            _rb.velocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            bool isMoving = targetVelocity.sqrMagnitude > 0.05f || _rb.velocity.sqrMagnitude > 0.05f;
            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetBool(IsMovingHash, isMoving);
        }

        private bool _isAtTarget; // New: To track state with hysteresis

        private bool CheckReachedTarget(Vector3 destination)
        {
            float currentDist = 0f;
            if (_followTarget != null)
            {
                var targetCollider = _followTarget.GetComponent<Collider2D>();
                if (targetCollider && _myCollider)
                {
                    if (_myCollider.IsTouching(targetCollider)) return true;
                    var result = Physics2D.Distance(_myCollider, targetCollider);
                    currentDist = result.distance;
                }
                else
                {
                    currentDist = Vector2.Distance(transform.position, destination);
                }
            }
            else
            {
                currentDist = Vector2.Distance(transform.position, destination);
            }

            // [SENIOR FIX] Hysteresis Logic
            // If already at target, we need more distance to start moving again
            // If not at target, we need to be within stop distance to stop
            float buffer = 0.2f;
            if (_isAtTarget)
            {
                if (currentDist > _stopDistance + buffer)
                {
                    _isAtTarget = false;
                }
            }
            else
            {
                if (currentDist <= _stopDistance)
                {
                    _isAtTarget = true;
                }
            }

            return _isAtTarget;
        }

        private void EnsureAgentOnNavMesh()
        {
            if (!_agent) return;
            if (_agent.isOnNavMesh) return;

            // [Senior Fix] Tăng bán kính tìm kiếm lên 10m để bao phủ các vùng spawn xa NavMesh
            if (NavMesh.SamplePosition(transform.position, out var hit, 10f, NavMesh.AllAreas))
            {
                _agent.enabled = true; // Đảm bảo agent đang bật
                _agent.Warp(hit.position);
                
                if (_agent.isOnNavMesh)
                {
                    Debug.Log($"[EnemyMovementController] {gameObject.name} successfully warped to NavMesh at {hit.position}");
                }
            }
            else
            {
                Debug.LogWarning($"[EnemyMovementController] {gameObject.name} could not find NavMesh within 10m!");
            }
        }

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }

        public void SetBaseMoveSpeed(float speed) => baseMoveSpeed = speed;
        public void SetSpeedMultiplier(float multiplier) => _speedMultiplier = multiplier;
        public float GetCurrentMoveSpeed() => Mathf.Max(0.1f, baseMoveSpeed * _speedMultiplier);
    }
}
