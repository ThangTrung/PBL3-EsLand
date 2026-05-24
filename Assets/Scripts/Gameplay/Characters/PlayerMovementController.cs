using Core.Contracts.Equipment;
using Core.Contracts.AI;
using System.Collections.Generic;
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
        private float _speedMultiplier = 1f;
        private Collider2D _myCollider;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        private NavMeshAgent _agent;
        private Vector3 _targetPosition;
        private bool _isMovingToClickPos = false; 

        public bool IsFollowingTarget => _followTarget != null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
            _myCollider = GetComponent<Collider2D>();
            _agent = GetComponent<NavMeshAgent>();

            if (_agent != null)
            {
                // CRITICAL for 2D Physics: Disable auto-movement and rotation
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                _agent.updateUpAxis = false;
            }
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

            bool isUsingAI = _isMovingToClickPos || _followTarget != null;

            if (isUsingAI && _agent != null)
            {
                // 1. Sync Agent with real world position (Manually because updatePosition is false)
                _agent.nextPosition = transform.position;

                // 2. Handle Follow Target (Dynamic)
                if (_followTarget != null)
                {
                    if (CheckReachedTarget())
                    {
                        CompleteFollow();
                        return;
                    }
                    _agent.SetDestination(_followTarget.position);
                }

                // 3. Handle Click Movement (Static)
                if (_isMovingToClickPos && _agent.remainingDistance <= 0.1f)
                {
                    StopClickMovement();
                    return;
                }

                // 4. Calculate Direction based on NavMesh steering target
                Vector2 steeringPos = _agent.steeringTarget;
                float distToSteering = Vector2.Distance(transform.position, steeringPos);
                
                if (distToSteering > 0.1f)
                {
                    Vector2 direction = (steeringPos - (Vector2)transform.position).normalized;
                    ApplyVelocity(direction * GetMoveSpeed());
                }
                else
                {
                    // Nếu quá gần steering target (điểm trung gian), tạm dừng hoặc chờ waypoint tiếp theo
                    if (!_isMovingToClickPos && _followTarget == null)
                        StopMovement();
                }
            }
        }

        /// <summary>
        /// Moves the character in a specific direction manually (WASD).
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                StopMovement();
                return;
            }

            // Manual input cancels any AI-driven movement
            if (direction.sqrMagnitude > 0.01f)
            {
                if (_followTarget != null) CancelFollow();
                if (_isMovingToClickPos) _isMovingToClickPos = false;
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
            }

            ApplyVelocity(direction.normalized * GetMoveSpeed());
        }

        // ==========================================
        // MOUSE INTERACTION INTEGRATION
        // ==========================================
        public void SetTargetPosition()
        {
            if (Camera.main == null) return;
            _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _targetPosition.z = transform.position.z;
        }

        public void SetAgentPosition() 
        {
            if (!_canMove || _agent == null) return;

            if (_followTarget != null) CancelFollow();

            EnsureAgentOnNavMesh();
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_targetPosition);
                _isMovingToClickPos = true;
            }
        }
        // ==========================================

        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            _isMovingToClickPos = false;
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

        public void CancelFollow()
        {
            _followTarget = null;
            _onTargetReached = null;
            if (_agent != null && _agent.isOnNavMesh && !_isMovingToClickPos) 
                _agent.isStopped = true;
        }

        private void StopClickMovement()
        {
            _isMovingToClickPos = false;
            if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
            StopMovement();
        }

        public void StopMovement() => ApplyVelocity(Vector2.zero);

        private void ApplyVelocity(Vector2 velocity)
        {
            if (_rb == null) return;
            
            _rb.velocity = velocity;
            
            bool isMoving = velocity.sqrMagnitude > 0.01f;
            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetBool(IsMovingHash, isMoving);

            // TĂNG THƯỞNG: Chỉ quay đầu khi vận tốc X thực sự đáng kể (tránh giật lắc khi tới đích)
            if (isMoving && Mathf.Abs(velocity.x) > 0.1f) 
                transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
        }

        private bool CheckReachedTarget()
        {
            if (_followTarget == null) return true;

            var targetCollider = _followTarget.GetComponent<Collider2D>();
            if (targetCollider != null && _myCollider != null)
            {
                return _myCollider.IsTouching(targetCollider) || 
                       Vector2.Distance(transform.position, _followTarget.position) <= _stopDistance;
            }
            
            return Vector2.Distance(transform.position, _followTarget.position) <= _stopDistance; 
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
            if (_agent == null) return;
            if (!_agent.isOnNavMesh)
            {
                // Try to snap the agent to the nearest point on the NavMesh
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
            }
        }

        public void SetSpeedMultiplier(float multiplier) => _speedMultiplier = multiplier;
        public float GetCurrentMoveSpeed() => GetMoveSpeed();
        public void SetBaseMoveSpeed(float speed) => baseMoveSpeed = speed;
        public void SetCanMove(bool canMove) { _canMove = canMove; if (!canMove) StopMovement(); }

        private float GetMoveSpeed()
        {
            float speed = baseMoveSpeed;
            if (_facade != null && _facade.EquipmentManager != null)
                speed += _facade.EquipmentManager.GetTotalSpeedModifier();
            
            return Mathf.Max(0.1f, speed * _speedMultiplier);
        }

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }
    }
}