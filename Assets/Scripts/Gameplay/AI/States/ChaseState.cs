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
        private Vector3 _lastTargetPos;
        private float _updateTimer;
        private const float UpdateInterval = 0.2f; // Update path every 0.2s
        private const float TargetMoveThreshold = 0.5f;

        public void Enter(EnemyBase enemy)
        {
            if (enemy.Animator != null)
            {
                enemy.Animator.PlayRun();
            }

            if (enemy.Target != null)
            {
                _lastTargetPos = enemy.Target.position;
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
            var targetPos = enemy.Target.position;
            var distanceToTarget = Vector3.Distance(enemy.transform.position, targetPos);
            
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

            // [SENIOR OPTIMIZATION] Throttle follow commands
            // Only update the follow target if the target has moved significantly or enough time has passed
            _updateTimer += Time.deltaTime;
            float targetMoveDist = Vector3.Distance(targetPos, _lastTargetPos);

            if (_updateTimer >= UpdateInterval || targetMoveDist > TargetMoveThreshold)
            {
                _updateTimer = 0f;
                _lastTargetPos = targetPos;
                
                float stopDist = Mathf.Max(0.1f, enemy.AttackRange * 0.6f);
                enemy.FollowTarget(enemy.Target, stopDist);
            }

            enemy.FaceTarget();
        }

        public void Exit(EnemyBase enemy)
        {
            // Stop movement when leaving chase state
            enemy.StopMovement();
        }
    }
}