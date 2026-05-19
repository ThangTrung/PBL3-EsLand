using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class PatrolState : IAIState
    {
        private Vector3 _patrolPoint;

        public void Enter(EnemyBase enemy)
        {
            PickNewPatrolPoint(enemy);
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.Target != null)
            {
                var distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);
                if (distance <= enemy.Config.DetectionRange)
                {
                    enemy.ChangeState(enemy.CreateChaseState());
                    return;
                }
            }

            enemy.MoveTowardsPosition(_patrolPoint);

            if (Vector3.Distance(enemy.transform.position, _patrolPoint) <= enemy.PatrolReachDistance)
            {
                PickNewPatrolPoint(enemy);
            }
        }

        public void Exit(EnemyBase enemy)
        {
        }

        private void PickNewPatrolPoint(EnemyBase enemy)
        {
            var rand = Random.insideUnitSphere * enemy.Config.PatrolRadius;
            rand.z = 0f;
            _patrolPoint = enemy.transform.position + rand;
        }
    }
}
