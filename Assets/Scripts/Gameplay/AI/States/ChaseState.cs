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

            // [IMPROVEMENT] Prioritize direct attack strategy check. 
            // If the strategy says we can shoot, we shoot, even if not perfectly aligned on Y.
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

            // [FIX] More lenient Y-alignment for ranged attackers (based on distance)
            float yTolerance = (enemy.AttackRange > 4f) ? 1.2f : 0.6f;
            bool isYAligned = distY <= yTolerance; 

            // --- STABILITY FIX: Side Choice Hysteresis ---
            float realSide = (enemy.transform.position.x < enemy.Target.position.x) ? -1f : 1f;
            if (distX > 1.5f || _sidePreference == 0f) 
            {
                _sidePreference = realSide;
            }
            
            float offsetDirection = _sidePreference;
            // ---------------------------------------------

            float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
            
            // For ranged units, we want to stay around 70-80% of range
            float targetXDistance = (attackRange > 4f) ? attackRange * 0.8f : attackRange * 0.75f;
            targetXDistance += _randomXOffset;
            
            float targetY = yWithOffset + _randomYOffset;
            Vector3 flankTarget;

            if (!isYAligned)
            {
                // If we are already at a good X distance, just move vertically to align better
                if (Mathf.Abs(distX - targetXDistance) < 0.5f)
                {
                    flankTarget = new Vector3(enemy.transform.position.x, targetY, enemy.transform.position.z);
                }
                else
                {
                    // Move towards the ideal flanking spot
                    flankTarget = new Vector3(
                        enemy.Target.position.x + (offsetDirection * targetXDistance),
                        targetY,
                        enemy.transform.position.z
                    );
                }
            }
            else
            {
                // Already aligned on Y, just maintain ideal X distance
                flankTarget = new Vector3(
                    enemy.Target.position.x + (offsetDirection * targetXDistance),
                    targetY,
                    enemy.transform.position.z
                );
            }

            enemy.DebugTargetPosition = flankTarget;
            enemy.MoveTowardsPosition(flankTarget);
            
            if (distanceToTarget < 6f || enemy.AttackRange > 5f)
            {
                enemy.FaceTarget();
            }
        }

        public void Exit(EnemyBase enemy)
        {
        }
    }
}