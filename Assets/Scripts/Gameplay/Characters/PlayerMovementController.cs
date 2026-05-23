using Core.Contracts.Equipment;
using Core.Contracts.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles physical movement and path following for the character.
    /// Interface for manual movement (Move) and automatic target following.
    /// Uses ICharacterNavigator service for pathfinding.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
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

        // Navigation Service
        private ICharacterNavigator _navigator;

        public bool IsFollowingTarget => _followTarget != null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
            _myCollider = GetComponent<Collider2D>();
            _navigator = GetComponent<ICharacterNavigator>();
        }

        private void Start()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie += HandleDie;
        }

        private void OnDestroy()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie -= HandleDie;
        }

        private void FixedUpdate()
        {
            if (_followTarget == null || !_canMove) return;

            // Sync Navigator with current physical position
            _navigator?.SyncPosition();

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
            
            if (_navigator != null)
            {
                _navigator.SetDestination(target.position);
            }
        }

        private void FollowPath()
        {
            if (_navigator == null)
            {
                MoveTowards(_followTarget.position);
                return;
            }

            // Update destination in case target moved
            _navigator.SetDestination(_followTarget.position);

            // Get next direction from the navigation service
            Vector2 direction = _navigator.GetNextDirection();
            
            ApplyVelocity(direction * GetMoveSpeed());
        }

        public void CancelFollow()
        {
            _followTarget = null;
            _onTargetReached = null;
            _navigator?.Stop();
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

            return _navigator != null ? _navigator.IsAtDestination(_stopDistance) : Vector2.Distance(transform.position, _followTarget.position) <= _stopDistance;
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
