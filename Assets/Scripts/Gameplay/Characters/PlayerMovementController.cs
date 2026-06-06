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
    public class PlayerMovementController : MovementControllerBase
    {
        private void FixedUpdate()
        {
            if (!_canMove) return;

            Vector3? destination = null;
            if (_followTarget != null) destination = _followTarget.position;
            else if (_targetPosition.HasValue) destination = _targetPosition.Value;

            if (!destination.HasValue) return;

            if (CheckReachedTarget(destination.Value))
            {
                CompleteFollow();
                return;
            }

            MoveTowardsDestination(destination.Value);
        }

        private void MoveTowardsDestination(Vector3 destination)
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.nextPosition = transform.position;
                _agent.SetDestination(destination);

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
                Vector2 direction = ((Vector2)destination - (Vector2)transform.position).normalized;
                ApplyVelocity(direction * GetMoveSpeed());
            }
        }

        /// <summary>
        /// Moves the character in a specific direction manually (WASD).
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (!_canMove) return;

            if (direction.sqrMagnitude > 0.01f && (_followTarget != null || _targetPosition.HasValue))
            {
                StopMovement();
            }

            ApplyVelocity(direction.normalized * GetMoveSpeed());
        }

        public void SetFollowTarget(Transform target, float stopDistance, System.Action onReached)
        {
            if (target == null) return;
            
            _followTarget = target;
            _targetPosition = null;
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

        public void SetTargetPosition(Vector3 position, float stopDistance, System.Action onReached)
        {
            _targetPosition = position;
            _followTarget = null;
            _stopDistance = stopDistance;
            _onTargetReached = onReached;
            _canMove = true;

            EnsureAgentOnNavMesh();
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(position);
            }
        }

        protected override float GetMoveSpeed()
        {
            var speed = baseMoveSpeed;
            if (_facade && _facade.EquipmentManager != null)
                speed += _facade.EquipmentManager.GetTotalSpeedModifier();
            
            float survivalMultiplier = 1f;
            if (TryGetComponent<PlayerSurvivalController>(out var survival))
            {
                survivalMultiplier = survival.GetSpeedMultiplier();
            }

            return Mathf.Max(0.1f, speed * survivalMultiplier);
        }
    }
}
