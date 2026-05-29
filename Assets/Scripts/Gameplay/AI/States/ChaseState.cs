using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class ChaseState : IAIState
    {
        private float _randomYOffset;
        private float _sidePreference = 0f; // -1 for Left, 1 for Right, 0 for Unset
        private float _randomXOffset;
        private bool _isStalker;

        public void Enter(EnemyBase enemy)
        {
            _randomYOffset = Random.Range(-0.15f, 0.15f);
            _randomXOffset = Random.Range(0.05f, 0.2f);
            
            // PROFESSIONAL DESIGN: Check if enemy is a "Thief" type to enable Stalker behavior
            _isStalker = enemy.gameObject.name.Contains("Thief");

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

            float yTolerance = (enemy.AttackRange > 4f) ? 1.2f : 0.6f;
            bool isYAligned = distY <= yTolerance; 

            // --- STALKER LOGIC: Always stay behind player ---
            if (_isStalker && enemy.Target != null)
            {
                // Target is behind if they are on the opposite side of player's facing
                // Assuming player localScale.x > 0 means facing Right
                float playerFacing = Mathf.Sign(enemy.Target.localScale.x);
                _sidePreference = -playerFacing; // Aim for the back
            }
            else
            {
                float realSide = (enemy.transform.position.x < enemy.Target.position.x) ? -1f : 1f;
                if (distX > 1.5f || _sidePreference == 0f) 
                {
                    _sidePreference = realSide;
                }
            }
            
            float offsetDirection = _sidePreference;
            // ---------------------------------------------

            float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
            float targetXDistance = (attackRange > 4f) ? attackRange * 0.8f : attackRange * 0.75f;
            targetXDistance += _randomXOffset;
            
            float targetY = yWithOffset + _randomYOffset;
            Vector3 flankTarget;

            if (!isYAligned)
            {
                if (Mathf.Abs(distX - targetXDistance) < 0.5f)
                {
                    flankTarget = new Vector3(enemy.transform.position.x, targetY, enemy.transform.position.z);
                }
                else
                {
                    flankTarget = new Vector3(
                        enemy.Target.position.x + (offsetDirection * targetXDistance),
                        targetY,
                        enemy.transform.position.z
                    );
                }
            }
            else
            {
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