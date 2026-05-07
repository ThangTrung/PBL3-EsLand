using UnityEngine;

namespace Script.Entities.AI
{
    public class EnemyPatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;

        public void Enter(Enemy enemy)
        {
            PickNewPatrolPoint(enemy);
        }

        public void Execute(Enemy enemy)
        {
            if (enemy.Target != null)
            {
                var distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);
                if (distance <= enemy.DetectionRange)
                {
                    enemy.ChangeState(new EnemyChaseState());
                    return;
                }
            }

            enemy.MoveTowardsPosition(_patrolPoint);

            if (Vector3.Distance(enemy.transform.position, _patrolPoint) <= enemy.PatrolReachDistance)
                PickNewPatrolPoint(enemy);
        }

        public void Exit(Enemy enemy)
        {
        }

        private void PickNewPatrolPoint(Enemy enemy)
        {
            var rand = Random.insideUnitSphere * enemy.PatrolRadius;
            rand.z = 0f;
            _patrolPoint = enemy.transform.position + rand;
        }
    }
}
