using Core.Contracts.AI;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Movement
{
    public class SimpleMovementStrategy : IMovementStrategy
    {
        private readonly EnemyMovementController _movementController;
        private readonly CharacterAnimationController _animationController;
        private readonly Gameplay.Combat.StatusEffects.StatusEffectController _statusEffectController;

        public SimpleMovementStrategy(EnemyMovementController movementController, CharacterAnimationController animationController, Gameplay.Combat.StatusEffects.StatusEffectController statusEffectController)
        {
            _movementController = movementController;
            _animationController = animationController;
            _statusEffectController = statusEffectController;
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
