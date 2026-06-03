using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class MeleeAttackStrategy : BaseAttackStrategy
    {
        private readonly float _damage;
        private readonly int _attackTriggerFrame;

        public MeleeAttackStrategy(float damage, float range, float cooldown, CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source)
            : base(animator, selfTransform, cooldown, range, source)
        {
            _damage = damage;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
        }

        protected override void OnBeginAttack()
        {
            _hitApplied = false;
        }

        protected override void InternalApplyHit()
        {
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (!_hitApplied && currentFrame >= _attackTriggerFrame)
            {
                float totalDist = Vector3.Distance(_selfTransform.position, _target.position);
                
                // Allow a generous forgiveness radius so attacks don't arbitrarily miss if player moves slightly
                bool inHitRange = totalDist <= _range + 0.8f;

                if (inHitRange && _target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var victim))
                {
                    victim.TakeDamage(_damage, _source);
                }

                _hitApplied = true;
                _nextAttackTime = Time.time + _cooldown;
            }
        }

        protected override void OnEndAttack()
        {
            _hitApplied = false;
        }

        public override bool CanStartAttack(Transform target)
        {
            if (!base.CanStartAttack(target)) return false;

            // Base check handles distance <= _range. 
            // Additional custom checks can go here if needed, but simple distance is usually best for smooth gameplay.
            return true;
        }
    }
}