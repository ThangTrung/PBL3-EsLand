using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class AssassinAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;
        private readonly float _backstabMultiplier = 1.5f;

        private Transform _target;
        private bool _hitApplied;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public AssassinAttackStrategy(float damage, float range, float cooldown, CharacterAnimationController animator, 
            int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _animator = animator;
            _attackTriggerFrame = attackTriggerFrame;
            _selfTransform = selfTransform;
            _source = source;
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null) return;
            _target = target;
            _hitApplied = false;
            IsAttacking = true;
            _animator.PlayAttack();
            // Debug.Log($"[Assassin] Starting attack on {target.name}");
        }

        public void TryApplyHitIfReady()
        {
            InternalApplyHit();
            
            // [FIX] More robust finish detection: check animation state and finish flag
            if (_animator != null)
            {
                bool isAttackAnim = _animator.GetCurrentState() == AnimationStateNames.Attack;
                if (!isAttackAnim || _animator.IsCurrentAnimationFinished())
                {
                    EndAttack();
                }
            }
        }

        private void InternalApplyHit()
        {
            if (!IsAttacking || _target == null || _animator == null) return;
            if (_animator.GetCurrentState() != AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            
            // [FIX] Increased tolerance for trigger frame to avoid missing it due to frame skips
            if (_hitApplied || currentFrame < _attackTriggerFrame) return;

            // [FIX] Use 2D distance for 2D game to avoid Z issues
            Vector2 pos2D = _selfTransform.position;
            Vector2 target2D = _target.position;
            var distance = Vector2.Distance(pos2D, target2D);

            if (distance <= _range + 0.2f) // Slight buffer for hit detection
            {
                if (_target.TryGetComponent<IDamageable>(out var victim))
                {
                    float finalDmg = _damage;
                    
                    // Backstab logic
                    float thiefFacing = Mathf.Sign(_selfTransform.localScale.x);
                    float targetFacing = Mathf.Sign(_target.localScale.x);

                    if (Mathf.Approximately(thiefFacing, targetFacing))
                    {
                        finalDmg *= _backstabMultiplier;
                        // Debug.Log("[Assassin] Backstab Applied!");
                    }

                    victim.TakeDamage(finalDmg, _source);
                }
            }

            _hitApplied = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _target = null;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null || Time.time < _nextAttackTime) return false;
            
            Vector2 pos2D = _selfTransform.position;
            Vector2 target2D = target.position;
            return Vector2.Distance(pos2D, target2D) <= _range;
        }
    }
}