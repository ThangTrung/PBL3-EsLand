using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.Characters;
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

        public void Move(Vector3 direction)
        {
            if (_movementController == null) return;

            if (_statusEffectController != null)
            {
                _movementController.SetSpeedMultiplier(_statusEffectController.SpeedMultiplier);
            }

            _movementController.Move(direction);
            _animationController?.SetFacingByMove(direction);

            if (_animationController != null && _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack &&
                _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Death)
            {
                _animationController.PlayRun();
            }
        }

        public void StopMovement()
        {
            _movementController?.StopMovement();

            if (_animationController != null && _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack &&
                _animationController.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Death)
            {
                _animationController.PlayIdle();
            }
        }
    }
}
