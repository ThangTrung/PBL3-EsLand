using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class HammerSmashAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _aoeRadius;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private Transform _target;
        private bool _hitApplied;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public HammerSmashAttackStrategy(float damage, float range, float aoeRadius, float cooldown, 
            CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _aoeRadius = aoeRadius;
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
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _animator == null) return;
            if (_animator.GetCurrentState() != CharacterAnimationController.AnimState.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_hitApplied || currentFrame < _attackTriggerFrame) return;

            ApplyAOEDamage();
            _hitApplied = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        private void ApplyAOEDamage()
        {
            // Position smash in front of boss
            Vector3 smashPos = _selfTransform.position + (_selfTransform.localScale.x > 0 ? Vector3.right : Vector3.left) * 1.5f;
            
            var colliders = Physics2D.OverlapCircleAll(smashPos, _aoeRadius);
            foreach (var col in colliders)
            {
                if (col.transform == _selfTransform) continue;
                
                if (col.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(_damage, _source);
                }
            }
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _target = null;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null || Time.time < _nextAttackTime) return false;
            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}
