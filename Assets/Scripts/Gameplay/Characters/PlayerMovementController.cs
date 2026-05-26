using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles physical movement for the player using NavMesh as a pathfinding source.
    /// Movement is applied via Rigidbody2D to ensure proper physics interaction.
    /// Support both WASD and Mouse-Click navigation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;

        private Rigidbody2D _rb;
        private Character _facade;
        private const float SpeedMultiplier = 1f;
        private Collider2D _myCollider;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        private NavMeshAgent _agent;

        public bool IsFollowingTarget => _followTarget != null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
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
            if (!_canMove) return;

            if (!_followTarget || !_agent) return;
            _agent.nextPosition = transform.position;

            if (CheckReachedTarget())
            {
                CompleteFollow();
                return;
            }
                
            // 2. Handle Follow Target (Dynamic)
            _agent.SetDestination(_followTarget.position);

            // 3. Calculate Direction based on NavMesh steering target
            Vector2 steeringPos = _agent.steeringTarget;
            var distToSteering = Vector2.Distance(transform.position, steeringPos);
                
            if (distToSteering > 0.1f)
            {
                var direction = (steeringPos - (Vector2)transform.position).normalized;
                ApplyVelocity(direction * GetMoveSpeed());
            }
            else
            {
                // Prevent Rigidbody from sliding past the steering target and oscillating.
                ApplyVelocity(Vector2.zero);
            }
        }

        /// <summary>
        /// Moves the character in a specific direction manually (WASD).
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                // Removed StopMovement() to allow physical forces like Knockback to persist
                return;
            }

            // Manual input cancels any AI-driven movement
            if (direction.sqrMagnitude > 0.01f)
            {
                if (_followTarget) CancelFollow();
            }

            ApplyVelocity(direction.normalized * GetMoveSpeed());
        }

        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            _followTarget = target;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;

            EnsureAgentOnNavMesh();
            if (!_agent|| !_agent.isOnNavMesh) return;
            _agent.isStopped = false;
            _agent.SetDestination(target.position);
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove) StopMovement();
        }

        private void CancelFollow()
        {
            _followTarget = null;
            _onTargetReached = null;
            if (_agent && _agent.isOnNavMesh) 
                _agent.isStopped = true;
        }

        public void StopMovement() => ApplyVelocity(Vector2.zero);

        private void ApplyVelocity(Vector2 velocity)
        {
            if (!_rb) return;
            
            _rb.velocity = velocity;
            
            var isMoving = velocity.sqrMagnitude > 0.01f;
            if (_facade && _facade.Animator)
                _facade.Animator.SetBool(IsMovingHash, isMoving);

            if (isMoving && Mathf.Abs(velocity.x) > 0.1f) 
                transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
        }

        private bool CheckReachedTarget()
        {
            if (!_followTarget) return true;

            var targetCollider = _followTarget.GetComponent<Collider2D>();
            if (targetCollider && _myCollider)
            {
                if (_myCollider.IsTouching(targetCollider)) return true;
                
                // Measure distance between colliders' boundaries instead of center-to-center
                var result = Physics2D.Distance(_myCollider, targetCollider);
                if (result.distance <= _stopDistance) return true;
            }
            else if (Vector2.Distance(transform.position, _followTarget.position) <= _stopDistance)
            {
                return true;
            }

            // If the NavMesh has brought the character right to the edge of the NavMeshObstacle
            // and the target is reasonably close, we accept it as reached.
            if (!_agent|| !_agent.enabled || !_agent.isOnNavMesh || _agent.pathPending) return false;
            if (!(_agent.remainingDistance <= 0.1f)) return false;
            return Vector2.Distance(transform.position, _followTarget.position) <= Mathf.Max(_stopDistance, 1.5f);
        }

        private void CompleteFollow()
        {
            var callback = _onTargetReached;
            CancelFollow();
            StopMovement();
            callback?.Invoke();
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
        private float GetMoveSpeed()
        {
            var speed = baseMoveSpeed;
            if (_facade && _facade.EquipmentManager != null)
                speed += _facade.EquipmentManager.GetTotalSpeedModifier();
            
            return Mathf.Max(0.1f, speed * SpeedMultiplier);
        }

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }
    }
}