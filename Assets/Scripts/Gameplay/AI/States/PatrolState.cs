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
                float detectionRange = enemy.Config != null ? enemy.Config.DetectionRange : 10f;
                var distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);
                if (distance <= detectionRange)
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
            float patrolRadius = enemy.Config != null ? enemy.Config.PatrolRadius : 4f;
            var rand = Random.insideUnitSphere * patrolRadius;
            rand.z = 0f;
            _patrolPoint = enemy.transform.position + rand;
        }
    }
}