using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class ChaseState : IAIState
    {
        private const float HorizontalTolerance = 0.2f;

        public void Enter(EnemyBase enemy)
        {
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.Target.position);
            if (distanceToTarget > enemy.Config.DetectionRange)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            var yDifference = Mathf.Abs(enemy.Target.position.y - enemy.transform.position.y);
            if (distanceToTarget <= enemy.AttackRange * 0.9f && yDifference < HorizontalTolerance)
            {
                enemy.ChangeState(new AttackState());
                return;
            }

            float signX = Mathf.Sign(enemy.transform.position.x - enemy.Target.position.x);
            if (signX == 0) signX = 1;

            Vector3 flankTarget = enemy.Target.position + new Vector3(signX * (enemy.AttackRange * 0.8f), 0, 0);
            enemy.MoveTowardsPosition(flankTarget);
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}
