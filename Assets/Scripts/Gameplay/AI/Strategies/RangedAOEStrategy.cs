using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using UnityEngine;
using Infrastructure.Pooling;

namespace Gameplay.AI.Strategies
{
    public class RangedAOEStrategy : IAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly Projectile2D _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly float _range;
        private readonly float _cooldown;

        private Transform _target;
        private bool _projectileSpawned;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public RangedAOEStrategy(ProjectileSpec spec, Projectile2D projectilePrefab, CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, float range, float cooldown)
        {
            _spec = spec;
            _projectilePrefab = projectilePrefab;
            _animator = animator;
            _attackTriggerFrame = attackTriggerFrame;
            _selfTransform = selfTransform;
            _range = range;
            _cooldown = cooldown;
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null || target == null) return;
            _target = target;
            _projectileSpawned = false;
            IsAttacking = true;
            _animator.PlayAttack();
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _target == null || _animator == null) return;
            if (_animator.GetCurrentState() != CharacterAnimationController.AnimState.Attack) return;

            if (_projectileSpawned || _animator.GetCurrentFrameIndex() < _attackTriggerFrame) return;

            SpawnAOEProjectile();
            _projectileSpawned = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _target = null;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            return Time.time >= _nextAttackTime && Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }

        private void SpawnAOEProjectile()
        {
            var direction = (_target.position - _selfTransform.position).normalized;
            var spawnPos = _selfTransform.position + direction * 0.5f;

            if (_projectilePrefab != null)
            {
                var go = ObjectPoolManager.Instance.Get(_projectilePrefab.gameObject, spawnPos, Quaternion.identity);
                var proj = go.GetComponent<Projectile2D>();
                if (proj != null) proj.Initialize(_spec, _selfTransform, _target);
            }
        }
    }
}
