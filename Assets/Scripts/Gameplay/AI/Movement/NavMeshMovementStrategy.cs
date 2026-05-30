using Core.Contracts.AI;
using Gameplay.AI.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.AI.Movement
{
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
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                _agent.updateUpAxis = false;
            }
        }

        public void Move(Vector3 destination)
        {
            if (_movementController == null) return;
            _movementController.SetTargetPosition(destination);
        }

        public void StopMovement()
        {
            _movementController?.StopMovement();
        }
    }
}
