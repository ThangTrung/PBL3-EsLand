using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    /// <summary>
    /// AI giữ khoảng cách với mục tiêu (Dùng cho quái đánh xa/Shaman).
    /// Tối ưu hóa: Chỉ cập nhật đích đến khi vị trí lý tưởng thay đổi đáng kể.
    /// </summary>
    public class KeepDistanceState : IAIState
    {
        private const float MinRangeFactor = 0.6f;
        private Vector3 _lastDestination;

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
            
            // Ở trong vùng lý tưởng -> Tấn công
            if (distanceToTarget <= enemy.AttackRange && distanceToTarget >= minRange)
            {
                enemy.StopMovement();
                enemy.ChangeState(new AttackState());
                return;
            }

            Vector3 targetDest;
            // Quá gần -> Lùi lại
            if (distanceToTarget < minRange)
            {
                var awayDirection = (enemy.transform.position - enemy.Target.position).normalized;
                targetDest = enemy.transform.position + awayDirection * 2f;
            }
            else // Quá xa -> Tiến lại gần
            {
                targetDest = enemy.Target.position;
            }

            // Chỉ cập nhật đích đến nếu nó thay đổi đáng kể (> 0.5m)
            if (Vector3.Distance(targetDest, _lastDestination) > 0.5f)
            {
                _lastDestination = targetDest;
                enemy.MoveTowardsPosition(targetDest);
            }

            enemy.FaceTarget();
        }

        public void Exit(EnemyBase enemy)
        {
            enemy.StopMovement();
        }
    }
}