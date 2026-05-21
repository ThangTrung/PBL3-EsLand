using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class PokeAndRetreatStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly float _retreatDistance;
        private readonly CharacterAnimationController _animator;
        private readonly EnemyMovementController _movement;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private float _nextAttackTime;
        private bool _hitApplied;
        private Transform _target;

        public bool IsAttacking { get; private set; }

        public PokeAndRetreatStrategy(float damage, float range, float cooldown, float retreatDistance, CharacterAnimationController animator, EnemyMovementController movement, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _retreatDistance = retreatDistance;
            _animator = animator;
            _movement = movement;
            _selfTransform = selfTransform;
            _source = source;
        }

        public void BeginAttack(Transform target)
        {
            IsAttacking = true;
            _hitApplied = false;
            _target = target;
            if (_animator != null) _animator.PlayAttack();
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _hitApplied || _target == null) return;

            if (Vector3.Distance(_selfTransform.position, _target.position) <= _range)
            {
                if (_target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damage, _source);
                    _hitApplied = true;
                    Retreat();
                }
            }
        }

        private void Retreat()
        {
            if (_movement == null || _target == null) return;
            Vector3 retreatDir = (_selfTransform.position - _target.position).normalized;
            _movement.Move(retreatDir * _retreatDistance);
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
