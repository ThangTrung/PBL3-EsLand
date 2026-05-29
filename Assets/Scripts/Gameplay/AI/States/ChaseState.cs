using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class ChaseState : IAIState
    {
        private const float HorizontalTolerance = 0.2f;

        private float _randomYOffset;
        private float _sidePreference = 0f; // -1 for Left, 1 for Right, 0 for Unset
        private float _randomXOffset;

        public void Enter(EnemyBase enemy)
        {
            _randomYOffset = Random.Range(-0.15f, 0.15f);
            _randomXOffset = Random.Range(0.05f, 0.2f);
            
            // Decisively choose a side once per chase to prevent rapid flipping
            if (enemy.Target != null)
            {
                _sidePreference = (enemy.transform.position.x < enemy.Target.position.x) ? -1f : 1f;
            }

            if (enemy.Animator != null)
            {
                enemy.Animator.PlayRun();
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

            // [FIX] Attack check must be prioritized and have a slight buffer
            if (enemy.AttackStrategy != null)
            {
                if (enemy.AttackStrategy.CanStartAttack(enemy.Target))
                {
                    enemy.ChangeState(new AttackState());
                    return;
                }
            }

            float yWithOffset = enemy.Target.position.y;
            if (enemy.Config != null)
            {
                yWithOffset += enemy.Config.VerticalAlignmentOffset;
            }

            float distX = Mathf.Abs(enemy.transform.position.x - enemy.Target.position.x);
            float distY = Mathf.Abs(enemy.transform.position.y - yWithOffset);

            bool isYAligned = distY <= 0.6f; 

            // --- STABILITY FIX: Side Choice Hysteresis ---
            float realSide = (enemy.transform.position.x < enemy.Target.position.x) ? -1f : 1f;
            if (distX > 1.5f || _sidePreference == 0f) 
            {
                _sidePreference = realSide;
            }
            
            float offsetDirection = _sidePreference;
            // ---------------------------------------------

            // [FIX] Ensure flank target is WELL WITHIN attack range to avoid oscillation at the edge
            float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
            float targetXDistance = attackRange * 0.75f + _randomXOffset; // 75% of range + small random
            
            float targetY = yWithOffset + _randomYOffset;
            Vector3 flankTarget;

            if (!isYAligned)
            {
                if (distX < 0.2f)
                {
                    flankTarget = new Vector3(enemy.transform.position.x, targetY, enemy.transform.position.z);
                }
                else if (distX < targetXDistance - 0.2f)
                {
                    flankTarget = new Vector3(enemy.Target.position.x + (offsetDirection * targetXDistance), enemy.transform.position.y, enemy.transform.position.z);
                }
                else
                {
                    flankTarget = new Vector3(enemy.transform.position.x, targetY, enemy.transform.position.z);
                }
            }
            else
            {
                // Close in for the kill
                flankTarget = new Vector3(
                    enemy.Target.position.x + (offsetDirection * targetXDistance),
                    targetY,
                    enemy.transform.position.z
                );
            }

            enemy.DebugTargetPosition = flankTarget;
            enemy.MoveTowardsPosition(flankTarget);
            
            if (distanceToTarget < 5f)
            {
                enemy.FaceTarget();
            }
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}