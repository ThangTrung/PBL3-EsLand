using Core.Contracts.AI;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class ComboMeleeStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly int _comboCount;
        private readonly CharacterAnimationController _animator;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private float _nextAttackTime;
        private int _currentHits;
        private Transform _target;

        public bool IsAttacking { get; private set; }

        public ComboMeleeStrategy(float damage, float range, float cooldown, int comboCount, CharacterAnimationController animator, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _comboCount = comboCount;
            _animator = animator;
            _selfTransform = selfTransform;
            _source = source;
        }

        public void BeginAttack(Transform target)
        {
            IsAttacking = true;
            _currentHits = 0;
            _target = target;
            if (_animator != null) _animator.PlayAttack();
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
            if (!IsAttacking || _target == null || _currentHits >= _comboCount) return;

            if (Vector3.Distance(_selfTransform.position, _target.position) <= _range)
            {
                if (_target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damage, _source);
                    _currentHits++;
                }
            }
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _nextAttackTime = Time.time + _cooldown;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            return Time.time >= _nextAttackTime && Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}
