using Core.Contracts.AI;
using Gameplay.AI.Movement;
using UnityEngine;

namespace Gameplay.AI.States
{
    /// <summary>
    /// AI đi tuần tra quanh vị trí hiện tại.
    /// Tối ưu hóa: Chỉ cập nhật điểm tuần tra mới khi đã tới đích.
    /// </summary>
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

            // [FIX] Kiểm tra xem Controller có đang dẫn đường không. Nếu không, hãy ra lệnh di chuyển.
            var moveController = enemy.GetComponent<EnemyMovementController>();
            if (moveController != null && !moveController.IsNavigating)
            {
                enemy.MoveTowardsPosition(_patrolPoint);
                if (enemy.Animator != null) enemy.Animator.PlayRun();
            }

            // [FIX] Sử dụng khoảng cách an toàn (PatrolReachDistance) đã được đồng bộ với Controller
            float distToPoint = Vector3.Distance(enemy.transform.position, _patrolPoint);
            if (distToPoint <= enemy.PatrolReachDistance)
            {
                PickNewPatrolPoint(enemy);
            }
        }

        public void Exit(EnemyBase enemy)
        {
            enemy.StopMovement();
        }

        private void PickNewPatrolPoint(EnemyBase enemy)
        {
            float patrolRadius = enemy.Config != null ? enemy.Config.PatrolRadius : 4f;
            float minDistance = 2f; // [FIX] Đảm bảo điểm mới không quá gần điểm cũ
            
            Vector3 randDir;
            int attempts = 0;
            do {
                randDir = Random.insideUnitSphere * patrolRadius;
                randDir.z = 0f;
                attempts++;
            } while (randDir.magnitude < minDistance && attempts < 10);

            _patrolPoint = enemy.transform.position + randDir;
        }
    }
}