using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    /// <summary>
    /// AI State for handling knockback and hit stun.
    /// Ensures the enemy stops chasing/attacking while taking damage.
    /// </summary>
    public class HitState : IAIState
    {
        private float _hitEndTime;
        private const float HitStunDuration = 0.35f; // Duration should encompass the knockback + brake time

        public void Enter(EnemyBase enemy)
        {
            // Ngừng mọi hoạt động di chuyển và logic đánh
            enemy.StopMovement();
            _hitEndTime = Time.time + HitStunDuration;
            
            // Note: Visual flash và Physics knockback vẫn do CombatFeedbackController đảm nhận.
            // Trạng thái này chỉ nhằm mục đích báo cho FSM biết "Đang bị choáng, đừng làm gì cả".
        }

        public void Execute(EnemyBase enemy)
        {
            // Chờ đến khi hết thời gian choáng (knockback kết thúc)
            if (Time.time >= _hitEndTime)
            {
                if (enemy.HasValidTarget)
                {
                    enemy.ChangeState(enemy.CreateChaseState());
                }
                else
                {
                    enemy.ChangeState(new PatrolState());
                }
            }
        }

        public void Exit(EnemyBase enemy)
        {
            // Trạng thái choáng kết thúc, EnemyBase sẽ tự động chạy tiếp logic của state mới
        }
    }
}
