using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class AttackState : IAIState
    {
        private bool _attackFinished;

        public void Enter(EnemyBase enemy)
        {
            enemy.StopMovement();
            enemy.FaceTarget();
            
            _attackFinished = false;
            enemy.AttackStrategy?.BeginAttack(enemy.Target);
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            // Let the strategy handle its internal logic (Windup, Attack, Recovery, Hit application)
            enemy.AttackStrategy?.TryApplyHitIfReady();

            // SOLID: The state machine defers to the strategy to know when the attack sequence is fully complete.
            if (enemy.AttackStrategy != null && !enemy.AttackStrategy.IsAttacking)
            {
                if (!_attackFinished)
                {
                    _attackFinished = true;
                }

                // If cooldown is ready and target is in range, attack again.
                if (enemy.AttackStrategy.CanStartAttack(enemy.Target))
                {
                    enemy.AttackStrategy.BeginAttack(enemy.Target);
                    _attackFinished = false;
                    return;
                }

                // Otherwise, go back to chasing or observing
                enemy.ChangeState(enemy.CreateChaseState());
            }
        }

        public void Exit(EnemyBase enemy)
        {
            if (enemy.AttackStrategy != null && enemy.AttackStrategy.IsAttacking)
            {
                enemy.AttackStrategy.EndAttack();
            }
        }
    }
}
