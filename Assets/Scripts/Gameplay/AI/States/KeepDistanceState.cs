using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class KeepDistanceState : IAIState
    {
        private const float MinRangeFactor = 0.6f;

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

            var minRange = enemy.AttackRange * MinRangeFactor;
            if (distanceToTarget <= enemy.AttackRange && distanceToTarget >= minRange)
            {
                enemy.StopMovement();
                enemy.ChangeState(new AttackState());
                return;
            }

            if (distanceToTarget < minRange)
            {
                var awayDirection = (enemy.transform.position - enemy.Target.position).normalized;
                var retreatPoint = enemy.transform.position + awayDirection;
                enemy.MoveTowardsPosition(retreatPoint);
                return;
            }

            enemy.MoveTowardsPosition(enemy.Target.position);
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}
