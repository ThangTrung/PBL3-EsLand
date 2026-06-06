using Gameplay.AI.Animation;
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
        private Collider2D _myCollider;
        private NavMeshAgent _agent;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        private float _nextFlipTime;
        private const float FlipCooldown = 0.15f;
        private const float SpeedMultiplier = 1f;

        public bool IsFollowingTarget => _followTarget != null;
        public bool IsKnockedBack { get; set; } // [NEW] Track knockback state externally

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
            _myCollider = GetComponent<Collider2D>();
            _agent = GetComponent<NavMeshAgent>();

            InitializeAgent();
        }

        private void InitializeAgent()
        {
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

            if (!_followTarget) return;

            if (CheckReachedTarget())
            {
                CompleteFollow();
                return;
            }

            MoveTowardsFollowTarget();
        }

        private void MoveTowardsFollowTarget()
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.nextPosition = transform.position;
                _agent.SetDestination(_followTarget.position);

                Vector2 steeringPos = _agent.steeringTarget;
                var distToSteering = Vector2.Distance(transform.position, steeringPos);
                    
                if (distToSteering > 0.1f)
                {
                    var direction = (steeringPos - (Vector2)transform.position).normalized;
                    ApplyVelocity(direction * GetMoveSpeed());
                }
                else
                {
                    ApplyVelocity(Vector2.zero);
                }
            }
            else
            {
                // Fallback to straight-line movement
                Vector2 direction = ((Vector2)_followTarget.position - (Vector2)transform.position).normalized;
                ApplyVelocity(direction * GetMoveSpeed());
            }
        }

        /// <summary>
        /// Moves the character in a specific direction manually (WASD).
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (!_canMove) return;

            if (direction.sqrMagnitude > 0.01f && _followTarget != null)
            {
                CancelFollow();
            }

            ApplyVelocity(direction.normalized * GetMoveSpeed());
        }

        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            if (target == null) return;
            
            _followTarget = target;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;

            EnsureAgentOnNavMesh();
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(target.position);
            }
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
            if (_agent != null && _agent.isOnNavMesh) 
                _agent.isStopped = true;
        }

        public void StopMovement() => ApplyVelocity(Vector2.zero);

        private void ApplyVelocity(Vector2 velocity)
        {
            if (!_rb) return;
            
            _rb.velocity = velocity;
            
            UpdateAnimation(velocity);
            UpdateFacing(velocity);
        }

        private void UpdateAnimation(Vector2 velocity)
        {
            if (_facade == null || _facade.Animator == null) return;
            
            bool isMoving = velocity.sqrMagnitude > 0.01f;
            _facade.Animator.SetBool(IsMovingHash, isMoving);
        }

        private void UpdateFacing(Vector2 velocity)
        {
            if (IsKnockedBack) return; // [FIX] Don't update facing during knockback to prevent unintended flipping
            if (Time.time < _nextFlipTime) return;
            if (Mathf.Abs(velocity.x) < 0.1f) return;

            float targetScaleX = Mathf.Sign(velocity.x);
            if (!Mathf.Approximately(transform.localScale.x, targetScaleX))
            {
                transform.localScale = new Vector3(targetScaleX, 1, 1);
                _nextFlipTime = Time.time + FlipCooldown;
            }
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
            
            float survivalMultiplier = 1f;
            if (TryGetComponent<PlayerSurvivalController>(out var survival))
            {
                survivalMultiplier = survival.GetSpeedMultiplier();
            }

            return Mathf.Max(0.1f, speed * SpeedMultiplier * survivalMultiplier);
        }

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }
    }
}