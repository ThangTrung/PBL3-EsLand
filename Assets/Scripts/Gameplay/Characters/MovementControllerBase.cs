using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Characters
{
    /// <summary>
    /// Base class for all character movement (Player and Enemy).
    /// Handles NavMesh navigation, Rigidbody2D physics, and basic animation state.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class MovementControllerBase : MonoBehaviour
    {
        protected static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Movement Base Settings")]
        [SerializeField] protected float baseMoveSpeed = 5f;
        [SerializeField] protected float flipCooldown = 0.15f;
        [SerializeField] protected float minFlippingThreshold = 0.1f;

        protected Rigidbody2D _rb;
        protected NavMeshAgent _agent;
        protected Character _facade;
        protected Collider2D _myCollider;

        protected bool _canMove = true;
        protected Transform _followTarget;
        protected Vector3? _targetPosition;
        protected float _stopDistance = 0.5f;
        protected System.Action _onTargetReached;

        protected float _nextFlipTime;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _agent = GetComponent<NavMeshAgent>();
            _facade = GetComponent<Character>();
            _myCollider = GetComponent<Collider2D>();

            InitializeAgent();
        }

        private void InitializeAgent()
        {
            if (_agent == null) return;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        protected virtual void Start()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie += HandleDie;
            
            EnsureAgentOnNavMesh();
        }

        protected virtual void OnDestroy()
        {
            if (_facade != null && _facade.Health != null)
                _facade.Health.OnDie -= HandleDie;
        }

        public bool IsFollowingTarget => _followTarget != null || _targetPosition.HasValue;

        public virtual void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove) StopMovement();
        }

        public virtual void StopMovement()
        {
            _followTarget = null;
            _targetPosition = null;
            _onTargetReached = null;
            if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
            ApplyVelocity(Vector2.zero);
        }

        protected virtual void ApplyVelocity(Vector2 velocity)
        {
            if (!_rb) return;
            _rb.velocity = velocity;
            UpdateAnimation(velocity);
            UpdateFacing(velocity.x);
        }

        protected virtual void UpdateAnimation(Vector2 velocity)
        {
            if (_facade == null || _facade.Animator == null) return;
            bool isMoving = velocity.sqrMagnitude > 0.01f;
            _facade.Animator.SetBool(IsMovingHash, isMoving);
        }

        protected virtual void UpdateFacing(float directionX)
        {
            if (Time.time < _nextFlipTime) return;
            if (Mathf.Abs(directionX) < minFlippingThreshold) return;

            float targetScaleX = Mathf.Sign(directionX);
            if (!Mathf.Approximately(transform.localScale.x, targetScaleX))
            {
                transform.localScale = new Vector3(targetScaleX, 1, 1);
                _nextFlipTime = Time.time + flipCooldown;
            }
        }

        protected bool CheckReachedTarget(Vector3 destination)
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
            
            return Vector2.Distance(transform.position, destination) <= _stopDistance;
        }

        protected void CompleteFollow()
        {
            var callback = _onTargetReached;
            StopMovement();
            callback?.Invoke();
        }

        protected void EnsureAgentOnNavMesh()
        {
            if (!_agent || _agent.isOnNavMesh) return;
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }

        protected abstract float GetMoveSpeed();

        public float GetCurrentMoveSpeed() => GetMoveSpeed();

        private void HandleDie()
        {
            _canMove = false;
            StopMovement();
        }
    }
}
