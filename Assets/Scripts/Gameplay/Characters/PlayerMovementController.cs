using Core.Contracts.Equipment;
using UnityEngine;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;

        private Rigidbody2D _rb;
        private Character _facade;

        private bool _canMove = true;
        private Transform _followTarget;
        private float _stopDistance;
        private System.Action _onTargetReached;

        public bool IsFollowingTarget => _followTarget != null;


        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            _followTarget = target;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;
        }

        public void CancelFollow()
        {
            _followTarget = null;
            _onTargetReached = null;
            // Removed StopMovement() to prevent overriding manual movement velocity
        }

        private void FixedUpdate()
        {
            if (_followTarget == null || !_canMove) return;

            // Check if we are touching the target's collider
            bool reached = false;
            var targetCollider = _followTarget.GetComponent<Collider2D>();
            var myCollider = GetComponent<Collider2D>();

            if (targetCollider != null && myCollider != null)
            {
                reached = myCollider.IsTouching(targetCollider) || Vector2.Distance(myCollider.bounds.ClosestPoint(transform.position), targetCollider.bounds.ClosestPoint(transform.position)) < 0.1f;
            }
            else
            {
                float distance = Vector2.Distance(transform.position, _followTarget.position);
                reached = (distance <= _stopDistance);
            }

            if (reached)
            {
                var callback = _onTargetReached;
                _followTarget = null;
                _onTargetReached = null;
                StopMovement();
                callback?.Invoke();
            }
            else
            {
                Vector3 direction = (_followTarget.position - transform.position).normalized;
                var movement = direction * GetMoveSpeed();
                _rb.velocity = new Vector2(movement.x, movement.y);

                if (direction.x != 0)
                {
                    transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
                }
                
                if (_facade.Animator != null)
                {
                    _facade.Animator.SetBool(IsMovingHash, true);
                }
            }
        }


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _facade = GetComponent<Character>();
        }

        private void Start()
        {
            if (_facade.Health != null)
            {
                _facade.Health.OnDie += HandleDie;
            }
        }

        private void OnDestroy()
        {
            if (_facade.Health != null)
            {
                _facade.Health.OnDie -= HandleDie;
            }
        }

        private void HandleDie()
        {
            StopMovement();
            _canMove = false;
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove)
            {
                StopMovement();
            }
        }

        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                StopMovement();
                return;
            }

            // If we are manually moving, cancel any automatic follow target
            if (direction.sqrMagnitude > 0.01f && _followTarget != null)
            {
                CancelFollow();
            }

            var movement = direction.normalized * GetMoveSpeed();
            _rb.velocity = new Vector2(movement.x, movement.y);

            var isMoving = direction.sqrMagnitude > 0.01f;
            if (_facade.Animator != null)
            {
                _facade.Animator.SetBool(IsMovingHash, isMoving);
            }

            if (direction.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
            }
        }

        public void StopMovement()
        {
            if (_rb != null) _rb.velocity = Vector2.zero;
            if (_facade.Animator != null) _facade.Animator.SetBool(IsMovingHash, false);
        }

        private float GetMoveSpeed()
        {
            var speed = baseMoveSpeed;
            if (_facade.EquipmentManager != null)
                speed += _facade.EquipmentManager.GetTotalSpeedModifier();
            return Mathf.Max(1f, speed);
        }
    }
}
