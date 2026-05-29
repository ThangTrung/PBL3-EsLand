using Core.Contracts.AI;
using Gameplay.AI.Enemies;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class BossTransitionState : IAIState
    {
        private float _transitionTimer;
        private readonly float _duration = 2.5f;

        public void Enter(EnemyBase enemy)
        {
            enemy.StopMovement();
            
            // Play Roar animation (assuming Attack state used for roar or special trigger)
            enemy.Animator?.PlayAttack(); 
            _transitionTimer = 0f;
        }

        public void Execute(EnemyBase enemy)
        {
            _transitionTimer += Time.deltaTime;
            
            if (_transitionTimer >= _duration || (enemy.Animator != null && enemy.Animator.IsCurrentAnimationFinished()))
            {
                if (enemy is OgreBossEnemy boss)
                {
                    boss.CompletePhaseTransition();
                }
            }
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}
