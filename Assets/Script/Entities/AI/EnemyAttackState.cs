using UnityEngine;

namespace Script.Entities.AI
{
    public class EnemyAttackState : IEnemyState
    {
        public void Enter(Enemy enemy)
        {
            enemy.StopMovement();
            enemy.Attack();
        }

        public void Execute(Enemy enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new EnemyPatrolState());
                return;
            }

            var distance = Vector3.Distance(enemy.transform.position, enemy.Target.position);

            if (distance > enemy.AttackRange)
            {
                enemy.ChangeState(new EnemyChaseState());
                return;
            }

            // Return to chase state to re-evaluate flanking and wait for cooldown
            enemy.ChangeState(new EnemyChaseState());
        }

        public void Exit(Enemy enemy)
        {
        }
    }
}
