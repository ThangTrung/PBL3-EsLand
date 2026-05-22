using Core.Contracts.Equipment;
using Core.Contracts.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles physical movement and path following for the character.
    /// Interface for manual movement (Move) and automatic target following.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float waypointTolerance = 0.1f;
        [SerializeField] private float repathThreshold = 0.5f;
        [SerializeField] private float repathCooldown = 0.2f;

        private Rigidbody2D _rb;
        private Character _facade;
        private float _speedMultiplier = 1f;

        private Collider2D _myCollider;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        // Pathfinding integration
        private IPathfinder _pathfinder;
        private List<Vector3> _currentPath;
        private int _currentWaypointIndex;
        private Vector3 _lastTargetPos;
        private float _lastRepathTime;
        private float _playerRadius = 0.3f;

        public bool IsFollowingTarget => _followTarget != null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
            _myCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie += HandleDie;

            // Tìm Pathfinder trên Scene
            _pathfinder = Object.FindAnyObjectByType<Core.Pathfinding.AStarPathfinder>();
            
            // Tính toán bán kính của Player từ Collider để Pathfinding check vật cản
            if (_myCollider != null)
            {
                _playerRadius = Mathf.Min(_myCollider.bounds.extents.x, _myCollider.bounds.extents.y) * 0.8f;
            }
        }

        private void OnDestroy()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie -= HandleDie;
        }

        private void FixedUpdate()
        {
            if (_followTarget == null || !_canMove) return;

            if (CheckReachedTarget())
            {
                CompleteFollow();
            }
            else
            {
                FollowPath();
            }
        }

        /// <summary>
        /// Moves the character in a specific direction manually.
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                StopMovement();
                return;
            }

            // Manual input cancels automatic following
            if (direction.sqrMagnitude > 0.01f && _followTarget != null)
            {
                CancelFollow();
            }

            ApplyVelocity(direction.normalized * GetMoveSpeed());
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }

        public float GetCurrentMoveSpeed() => GetMoveSpeed();

        public void SetBaseMoveSpeed(float speed)
        {
            baseMoveSpeed = speed;
        }

        /// <summary>
        /// Sets a target for the character to follow automatically.
        /// </summary>
        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            _followTarget = target;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;
            
            CalculatePath();
        }

        private void CalculatePath()
        {
            if (_pathfinder == null || _followTarget == null) return;
            
            _currentPath = _pathfinder.FindPath(transform.position, _followTarget.position, _playerRadius);
            _currentWaypointIndex = 0;
            _lastTargetPos = _followTarget.position;
            _lastRepathTime = Time.time;
        }

        private void FollowPath()
        {
            // 1. Kiểm tra xem target có di chuyển quá khoảng Threshold không để tính lại đường
            if (Time.time - _lastRepathTime > repathCooldown)
            {
                if (Vector3.Distance(_lastTargetPos, _followTarget.position) > repathThreshold)
                {
                    CalculatePath();
                }
            }

            // 2. Nếu không có path hoặc đã đi hết path, đi thẳng (fallback)
            if (_currentPath == null || _currentPath.Count == 0 || _currentWaypointIndex >= _currentPath.Count)
            {
                MoveTowards(_followTarget.position);
                return;
            }

            // 3. Di chuyển tới waypoint hiện tại
            Vector3 currentWaypoint = _currentPath[_currentWaypointIndex];
            
            // 4. Chuyển waypoint nếu đã tới đủ gần
            if (Vector2.Distance(transform.position, currentWaypoint) <= waypointTolerance)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= _currentPath.Count)
                {
                    MoveTowards(_followTarget.position);
                    return;
                }
                currentWaypoint = _currentPath[_currentWaypointIndex];
            }

            MoveTowards(currentWaypoint);
        }

        public void CancelFollow()
        {
            _followTarget = null;
            _onTargetReached = null;
            _currentPath = null;
        }

        public void StopMovement() => ApplyVelocity(Vector2.zero);

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            ApplyVelocity(direction * GetMoveSpeed());
        }

        private void ApplyVelocity(Vector2 velocity)
        {
            if (_rb == null) return;
            
            _rb.velocity = velocity;
            bool isMoving = velocity.sqrMagnitude > 0.01f;

            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetBool(IsMovingHash, isMoving);

            if (isMoving && Mathf.Abs(velocity.x) > 0.01f)
                transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
        }

        private bool CheckReachedTarget()
        {
            if (_followTarget == null) return true;

            var targetCollider = _followTarget.GetComponent<Collider2D>();
            if (targetCollider != null && _myCollider != null)
            {
                // Reached if touching or extremely close to bounds
                return _myCollider.IsTouching(targetCollider) || 
                       Vector2.Distance(_myCollider.bounds.ClosestPoint(transform.position), 
                                      targetCollider.bounds.ClosestPoint(transform.position)) < 0.1f;
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

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }

        private float GetMoveSpeed()
        {
            float speed = baseMoveSpeed;
            if (_facade != null && _facade.EquipmentManager != null)
                speed += _facade.EquipmentManager.GetTotalSpeedModifier();
            
            return Mathf.Max(0.1f, speed * _speedMultiplier);
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove) StopMovement();
        }
    }
}
