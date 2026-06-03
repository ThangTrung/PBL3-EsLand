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

            if (enemy.AttackStrategy != null && enemy.AttackStrategy.CanStartAttack(enemy.Target))
            {
                enemy.ChangeState(new AttackState());
                return;
            }

            // Always follow the target to stay in optimal range
            float stopDist = Mathf.Max(0.1f, enemy.AttackRange * 0.6f);
            enemy.FollowTarget(enemy.Target, stopDist);
            enemy.FaceTarget();
        }

        public void Exit(EnemyBase enemy)
        {
            // Stop movement when leaving chase state
            enemy.StopMovement();
        }
    }
}