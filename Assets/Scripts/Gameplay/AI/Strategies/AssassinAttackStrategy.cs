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
        private readonly float _backstabMultiplier = 1.3f;

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
        }

        public void TryApplyHitIfReady()
        {
            InternalApplyHit();
            if (_animator != null && _animator.IsCurrentAnimationFinished())
            {
                EndAttack();
            }
        }

        private void InternalApplyHit()
        {
            if (!IsAttacking || _target == null || _animator == null) return;
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_hitApplied || currentFrame < _attackTriggerFrame) return;

            var distance = Vector3.Distance(_selfTransform.position, _target.position);
            if (distance <= _range)
            {
                if (_target.TryGetComponent<IDamageable>(out var victim))
                {
                    float finalDmg = _damage;
                    
                    // Backstab logic: Check if Thief and Target are facing roughly the same direction
                    // Thief's forward is based on localScale.x (since it's 2D)
                    float thiefFacing = Mathf.Sign(_selfTransform.localScale.x);
                    float targetFacing = Mathf.Sign(_target.localScale.x);

                    if (Mathf.Approximately(thiefFacing, targetFacing))
                    {
                        finalDmg *= _backstabMultiplier;
                        Debug.Log("Backstab!");
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
            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}
