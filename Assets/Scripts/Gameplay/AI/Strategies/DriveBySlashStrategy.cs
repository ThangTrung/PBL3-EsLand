using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class DriveBySlashStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly float _speedBoost;
        private readonly CharacterAnimationController _animator;
        private readonly EnemyMovementController _movement;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private float _nextAttackTime;
        private bool _hitApplied;

        public bool IsAttacking { get; private set; }

        public DriveBySlashStrategy(float damage, float range, float cooldown, float speedBoost, CharacterAnimationController animator, EnemyMovementController movement, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _speedBoost = speedBoost;
            _animator = animator;
            _movement = movement;
            _selfTransform = selfTransform;
            _source = source;
        }

        public void BeginAttack(Transform target)
        {
            IsAttacking = true;
            _hitApplied = false;
            if (_movement != null) _movement.SetSpeedMultiplier(_speedBoost);
            if (_animator != null) _animator.PlayAttack();
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _hitApplied) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(_selfTransform.position, player.transform.position) <= _range)
            {
                if (player.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damage, _source);
                    _hitApplied = true;
                }
            }
        }

        public void EndAttack()
        {
            IsAttacking = false;
            if (_movement != null) _movement.SetSpeedMultiplier(1f);
            _nextAttackTime = Time.time + _cooldown;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            return Time.time >= _nextAttackTime && Vector3.Distance(_selfTransform.position, target.position) <= _range * 2f;
        }
    }
}
