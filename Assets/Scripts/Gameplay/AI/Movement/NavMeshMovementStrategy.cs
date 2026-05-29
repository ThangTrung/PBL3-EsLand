using Core.Contracts.AI;
using Gameplay.AI.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.AI.Movement
{
    /// <summary>
    /// Movement strategy that uses NavMeshAgent for pathfinding but applies movement via EnemyMovementController (Rigidbody2D).
    /// This avoids the "invisible" issue by disabling automatic agent rotation and position updates.
    /// </summary>
    public class NavMeshMovementStrategy : IMovementStrategy
    {
        private readonly EnemyMovementController _movementController;
        private readonly CharacterAnimationController _animationController;
        private readonly NavMeshAgent _agent;
        private readonly Gameplay.Combat.StatusEffects.StatusEffectController _statusEffectController;

        public NavMeshMovementStrategy(
            EnemyMovementController movementController, 
            CharacterAnimationController animationController, 
            NavMeshAgent agent,
            Gameplay.Combat.StatusEffects.StatusEffectController statusEffectController)
        {
            _movementController = movementController;
            _animationController = animationController;
            _agent = agent;
            _statusEffectController = statusEffectController;

            if (_agent != null)
            {
                // CRITICAL: Disable these to prevent "invisible" bug (flipping sprite) and fighting with Rigidbody2D
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                _agent.updateUpAxis = false;
            }
        }

        public void Move(Vector3 destination)
        {
            if (_movementController == null || _agent == null) return;

            if (!_agent.isOnNavMesh)
            {
                // Fallback to simple movement if not on NavMesh
                Vector3 direction = (destination - _movementController.transform.position).normalized;
                ApplySimpleMove(direction);
                return;
            }

            // Sync agent position with transform
            _agent.nextPosition = _movementController.transform.position;
            _agent.SetDestination(destination);

            if (_agent.pathPending) return;

            // Use steering target for the next move direction
            Vector3 steeringTarget = _agent.steeringTarget;
            Vector3 directionToTarget = (steeringTarget - _movementController.transform.position).normalized;

            if (_agent.remainingDistance > _agent.stoppingDistance)
            {
                ApplySimpleMove(directionToTarget);
            }
            else
            {
                StopMovement();
            }
        }

        private void ApplySimpleMove(Vector3 direction)
        {
            if (_statusEffectController != null)
            {
                _movementController.SetSpeedMultiplier(_statusEffectController.SpeedMultiplier);
            }

            _movementController.Move(direction);
            _animationController?.SetFacingByMove(direction);

            if (_animationController != null && 
                _animationController.GetCurrentState() != AnimationStateNames.Attack &&
                _animationController.GetCurrentState() != AnimationStateNames.Death)
            {
                _animationController.PlayRun();
            }
        }

        public void StopMovement()
        {
            _movementController?.StopMovement();
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }

            if (_animationController != null && 
                _animationController.GetCurrentState() != AnimationStateNames.Attack &&
                _animationController.GetCurrentState() != AnimationStateNames.Death)
            {
                _animationController.PlayIdle();
            }
        }
    }
}
