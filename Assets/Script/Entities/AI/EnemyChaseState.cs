using UnityEngine;

namespace Script.Entities.AI
{
    public class EnemyChaseState : IEnemyState
    {
        private const float HorizontalTolerance = 0.2f;

        public void Enter(Enemy enemy)
        {
        }

        public void Execute(Enemy enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new EnemyPatrolState());
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.Target.position);

            if (distanceToTarget > enemy.DetectionRange)
            {
                enemy.ChangeState(new EnemyPatrolState());
                return;
            }

            // Calculate vertical alignment for flanking strategy
            var yDifference = Mathf.Abs(enemy.transform.position.y - enemy.Target.position.y);
            var baseChaseDistance = enemy.AttackRange * 0.9f;

            // FLANKING LOGIC: Only attack if horizontally aligned
            if (distanceToTarget <= enemy.AttackRange && yDifference < HorizontalTolerance)
            {
                enemy.ChangeState(new EnemyAttackState());
                return;
            }

            // MOVEMENT LOGIC: Flank or approach based on vertical distance
            float signX = Mathf.Sign(enemy.transform.position.x - enemy.Target.position.x);
            if (signX == 0) signX = 1;

            float flankOffset = enemy.AttackRange * 0.8f;
            Vector3 flankTarget = enemy.Target.position + new Vector3(signX * flankOffset, 0, 0);

            // If far vertically, move directly towards player to close distance
            if (yDifference > 2f)
            {
                enemy.MoveTowardsPosition(enemy.Target.position);
            }
            else
            {
                // Close vertically, move to flanking position
                enemy.MoveTowardsPosition(flankTarget);
            }
        }

        public void Exit(Enemy enemy)
        {
        }
    }
}
