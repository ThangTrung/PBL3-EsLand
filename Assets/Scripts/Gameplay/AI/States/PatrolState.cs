using Core.Contracts.AI;
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
        private bool _isMovingToPoint;

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

            // Chỉ gọi lệnh di chuyển 1 lần duy nhất cho mỗi điểm tuần tra
            if (!_isMovingToPoint)
            {
                _isMovingToPoint = true;
                enemy.MoveTowardsPosition(_patrolPoint);
            }

            // Kiểm tra xem đã tới đích chưa để đổi điểm
            if (Vector3.Distance(enemy.transform.position, _patrolPoint) <= enemy.PatrolReachDistance)
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
            var rand = Random.insideUnitSphere * patrolRadius;
            rand.z = 0f;
            _patrolPoint = enemy.transform.position + rand;
            _isMovingToPoint = false; // Reset cờ để Execute sẽ gọi lệnh di chuyển mới
        }
    }
}