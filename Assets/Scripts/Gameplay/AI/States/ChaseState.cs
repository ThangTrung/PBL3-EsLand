using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class ChaseState : IAIState
    {
        private const float HorizontalTolerance = 0.2f;

        private float _randomYOffset;
        private float _randomXOffset;

        public void Enter(EnemyBase enemy)
        {
            _randomYOffset = Random.Range(-0.15f, 0.15f);
            _randomXOffset = Random.Range(0.1f, 0.4f);
            if (enemy.Animator != null)
            {
                enemy.Animator.PlayRun();
            }
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

            float yWithOffset = enemy.Target.position.y;
            if (enemy.Config != null)
            {
                yWithOffset += enemy.Config.VerticalAlignmentOffset;
            }

            float distX = Mathf.Abs(enemy.transform.position.x - enemy.Target.position.x);
            float distY = Mathf.Abs(enemy.transform.position.y - yWithOffset);
            
            // Dung sai được nới lỏng nhẹ để nhiều quái có thể cùng tấn công
            bool isYAligned = distY <= 0.25f; 
            bool isXInRange = distX <= enemy.AttackRange + 0.2f;

            if (isYAligned && isXInRange)
            {
                enemy.ChangeState(new AttackState());
                return;
            }

            float offsetDirection = (enemy.transform.position.x < enemy.Target.position.x) ? -1f : 1f;

            // X offset dạt ra xa đủ để né hitbox Player (Cộng thêm randomX để tụi nó không đứng đè lên nhau)
            float safeXOffset = enemy.AttackRange + _randomXOffset;
            if (safeXOffset < 0.8f) safeXOffset = 0.8f;

            float targetY = yWithOffset + _randomYOffset;
            Vector3 flankTarget;

            if (!isYAligned)
            {
                // Nếu đang đứng trên đầu/dưới chân mà khoảng cách X quá hẹp (nguy cơ kẹt đầu Player)
                if (distX < safeXOffset - 0.1f)
                {
                    // ƯU TIÊN 1: Chạy dạt ngang ra ngoài trước (Né Collider)
                    flankTarget = new Vector3(
                        enemy.Target.position.x + (offsetDirection * safeXOffset),
                        enemy.transform.position.y,
                        enemy.transform.position.z
                    );
                }
                else
                {
                    // ƯU TIÊN 2: Khi đã ở ngoài rìa an toàn, chạy thẳng xuống để dóng chuẩn trục Y
                    flankTarget = new Vector3(
                        enemy.transform.position.x,
                        targetY,
                        enemy.transform.position.z
                    );
                }
            }
            else
            {
                // ƯU TIÊN 3: Đã dóng chuẩn trục Y, chạy áp sát vào theo trục X để vụt
                flankTarget = new Vector3(
                    enemy.Target.position.x + (offsetDirection * (enemy.AttackRange - 0.1f)),
                    targetY,
                    enemy.transform.position.z
                );
            }
            
            enemy.DebugTargetPosition = flankTarget;
            enemy.MoveTowardsPosition(flankTarget);
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}
