using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    /// <summary>
    /// AI bám đuổi mục tiêu.
    /// Cập nhật: Luôn đảm bảo quái vật bám sát và nhìn về phía mục tiêu.
    /// </summary>
    public class ChaseState : IAIState
    {
        public void Enter(EnemyBase enemy)
        {
            if (enemy.Animator != null)
            {
                enemy.Animator.PlayRun();
            }

            if (enemy.Target != null)
            {
                // Kích hoạt bám đuổi lần đầu
                enemy.FollowTarget(enemy.Target, enemy.AttackRange * 0.8f);
            }
        }

        public void Execute(EnemyBase enemy)
        {
            if (!enemy.HasValidTarget)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            float detectionRange = enemy.Config != null ? enemy.Config.DetectionRange : 12f;
            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.Target.position);
            
            if (distanceToTarget > detectionRange)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            // Kiểm tra khả năng tấn công
            if (enemy.AttackStrategy != null)
            {
                if (enemy.AttackStrategy.CanStartAttack(enemy.Target))
                {
                    enemy.ChangeState(new AttackState());
                    return;
                }
            }

            // [STABILITY] Luôn gọi FollowTarget để đảm bảo AI 'tỉnh táo'.
            float stopDist = enemy.AttackRange * 0.6f;
            
            // [FIX] ÁP SÁT LINH HOẠT: 
            // Nếu đã vào tầm đánh (60%) mà chưa đánh được (do lệch hàng), 
            // hãy ép nó 'nhích' sát vào Player (stopDist = 0.1m) cho đến khi vung đòn được thì thôi.
            if (distanceToTarget <= stopDist)
            {
                stopDist = 0.1f;
            }

            enemy.FollowTarget(enemy.Target, stopDist);
            
            // Xoay mặt nhìn Player khi đứng gần (Hàm này đã có bảo vệ vận tốc bên trong EnemyBase)
            enemy.FaceTarget();
        }

        public void Exit(EnemyBase enemy)
        {
            // Stop movement when leaving chase state
            enemy.StopMovement();
        }
    }
}