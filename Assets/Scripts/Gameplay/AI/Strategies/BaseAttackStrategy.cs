using Core.Contracts.AI;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public abstract class BaseAttackStrategy : IAttackStrategy
    {
        protected readonly CharacterAnimationController _animator;
        protected readonly Transform _selfTransform;
        protected readonly float _cooldown;
        protected readonly float _range;
        protected readonly Gameplay.Characters.Character _source;

        protected Transform _target;
        protected float _nextAttackTime;
        protected bool _hitApplied;

        public bool IsAttacking { get; protected set; }

        protected BaseAttackStrategy(CharacterAnimationController animator, Transform selfTransform, float cooldown, float range, Gameplay.Characters.Character source = null)
        {
            _animator = animator;
            _selfTransform = selfTransform;
            _cooldown = cooldown;
            _range = range;
            _source = source;
        }

        public virtual void BeginAttack(Transform target)
        {
            if (_animator == null) return;
            
            _target = target;
            IsAttacking = true;

            OnBeginAttack();
            _animator.PlayAttack();
        }

        protected virtual void OnBeginAttack() { }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _target == null || _animator == null) return;

            InternalApplyHit();

            if (_animator.IsCurrentAnimationFinished())
            {
                EndAttack();
            }
        }

        protected abstract void InternalApplyHit();

        public virtual void EndAttack()
        {
            IsAttacking = false;
            _target = null;
            OnEndAttack();
        }

        protected virtual void OnEndAttack() { }

        public virtual bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            if (Time.time < _nextAttackTime) return false;

            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}