using UnityEngine;

namespace Gameplay.AI.Movement
{
    /// <summary>
    /// Handles physical movement for enemy entities.
    /// Separated from PlayerMovementController to avoid architectural coupling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 3f;
        private float _speedMultiplier = 1f;

        private Rigidbody2D _rb;
        private Gameplay.Characters.Character _facade;
        private Collider2D _myCollider;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Gameplay.Characters.Character>();
            _myCollider = GetComponent<Collider2D>();
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
                MoveTowards(_followTarget.position);
            }
        }

        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                // Removed StopMovement() to allow physical forces like Knockback to persist
                return;
            }

            ApplyVelocity(direction.normalized * GetCurrentMoveSpeed());
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }

        public float GetCurrentMoveSpeed()
        {
            float speed = baseMoveSpeed;
            return Mathf.Max(0.1f, speed * _speedMultiplier);
        }

        public void SetBaseMoveSpeed(float speed)
        {
            baseMoveSpeed = speed;
        }

        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            _followTarget = target;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;
        }

        public void StopMovement() => ApplyVelocity(Vector2.zero);

        private void MoveTowards(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            ApplyVelocity(direction * GetCurrentMoveSpeed());
        }

        private void ApplyVelocity(Vector2 velocity)
        {
            if (_rb == null) return;
            _rb.velocity = velocity;
            bool isMoving = velocity.sqrMagnitude > 0.01f;

            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetBool(IsMovingHash, isMoving);
        }

        private bool CheckReachedTarget()
        {
            if (_followTarget == null) return true;
            return Vector2.Distance(transform.position, _followTarget.position) <= _stopDistance;
        }

        private void CompleteFollow()
        {
            var callback = _onTargetReached;
            _followTarget = null;
            _onTargetReached = null;
            StopMovement();
            callback?.Invoke();
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove) StopMovement();
        }
    }
}