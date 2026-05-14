using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class AttackState : IAIState
    {
        public void Enter(EnemyBase enemy)
        {
            enemy.StopMovement();
            enemy.FaceTarget();
            enemy.AttackStrategy?.BeginAttack(enemy.Target);
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            enemy.AttackStrategy?.TryApplyHitIfReady();

            if (enemy.Animator != null && enemy.Animator.IsCurrentAnimationFinished())
            {
                enemy.AttackStrategy?.EndAttack();

                if (enemy.AttackStrategy != null && enemy.AttackStrategy.CanStartAttack(enemy.Target))
                {
                    enemy.AttackStrategy.BeginAttack(enemy.Target);
                    return;
                }

                enemy.ChangeState(enemy.CreateChaseState());
                return;
            }

            var distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);
            if (distance > enemy.AttackRange)
            {
                enemy.ChangeState(enemy.CreateChaseState());
            }
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}
