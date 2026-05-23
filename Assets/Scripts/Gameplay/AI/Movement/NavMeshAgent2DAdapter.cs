using Core.Contracts.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.AI.Movement
{
    /// <summary>
    /// Service that adapts 3D NavMeshAgent for 2D XY gameplay using XZ mapping.
    /// Manages the translation between 2D world space and 3D navigation space.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgent2DAdapter : MonoBehaviour, ICharacterNavigator
    {
        [Header("Settings")]
        [SerializeField] private float navMeshSampleDistance = 2.0f;
        [SerializeField] private float repathCooldown = 0.5f;

        private NavMeshAgent _agent;
        private float _lastRepathTime;
        private Vector2 _currentDestination;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            
            // Critical for 2D integration
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        public void SetDestination(Vector2 destination)
        {
            _currentDestination = destination;
            if (Time.time - _lastRepathTime > repathCooldown)
            {
                UpdateAgentDestination();
            }
        }

        private void UpdateAgentDestination()
        {
            if (_agent == null || !_agent.isActiveAndEnabled) return;

            if (EnsureAgentOnNavMesh())
            {
                // Map 2D destination (x, y) to 3D navigation destination (x, 0, y)
                Vector3 targetNavPos = new Vector3(_currentDestination.x, 0, _currentDestination.y);
                
                if (NavMesh.SamplePosition(targetNavPos, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    _agent.SetDestination(hit.position);
                }
                else
                {
                    _agent.SetDestination(targetNavPos);
                }
                _lastRepathTime = Time.time;
            }
        }

        public Vector2 GetNextDirection()
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return (_currentDestination - (Vector2)transform.position).normalized;
            }

            // Map agent's next position (x, 0, z) back to 2D (x, y)
            Vector2 currentPos2D = transform.position;
            Vector2 nextPos2D = new Vector2(_agent.nextPosition.x, _agent.nextPosition.z);
            Vector2 direction = (nextPos2D - currentPos2D).normalized;

            // If we are very close to next position but not at final target, look further at steering target
            if (direction.sqrMagnitude < 0.01f && _agent.hasPath)
            {
                Vector2 steering2D = new Vector2(_agent.steeringTarget.x, _agent.steeringTarget.z);
                direction = (steering2D - currentPos2D).normalized;
            }

            return direction;
        }

        public void Stop()
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
            }
        }

        public bool IsAtDestination(float stopDistance)
        {
            return Vector2.Distance(transform.position, _currentDestination) <= stopDistance;
        }

        public void SyncPosition()
        {
            if (_agent == null) return;

            // Map 2D world position (x, y) to 3D navigation position (x, 0, y)
            Vector3 navPos = new Vector3(transform.position.x, 0, transform.position.y);
            
            if (!_agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(navPos, out NavMeshHit hit, navMeshSampleDistance * 2f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
            }
            else
            {
                _agent.nextPosition = navPos;
            }
        }

        private bool EnsureAgentOnNavMesh()
        {
            if (_agent == null) return false;
            if (_agent.isOnNavMesh) return true;

            Vector3 navPos = new Vector3(transform.position.x, 0, transform.position.y);
            if (NavMesh.SamplePosition(navPos, out NavMeshHit hit, navMeshSampleDistance * 2f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
                return _agent.isOnNavMesh;
            }
            return false;
        }
    }
}
