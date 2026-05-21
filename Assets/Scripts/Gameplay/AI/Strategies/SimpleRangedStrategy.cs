using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class SimpleRangedStrategy : IAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly Projectile2D _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _triggerFrame;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly Transform _self;

        private float _nextAttackTime;
        private bool _spawned;
        private Transform _target;

        public bool IsAttacking { get; private set; }

        public SimpleRangedStrategy(ProjectileSpec spec, Projectile2D prefab, CharacterAnimationController animator, int triggerFrame, float range, float cooldown, Transform self)
        {
            _spec = spec;
            _projectilePrefab = prefab;
            _animator = animator;
            _triggerFrame = triggerFrame;
            _range = range;
            _cooldown = cooldown;
            _self = self;
        }

        public void BeginAttack(Transform target)
        {
            IsAttacking = true;
            _spawned = false;
            _target = target;
            if (_animator != null) _animator.PlayAttack();
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _spawned || _target == null || _animator == null) return;
            if (_animator.GetCurrentFrameIndex() < _triggerFrame) return;

            var direction = (_target.position - _self.position).normalized;
            if (_projectilePrefab != null)
            {
                var go = ObjectPoolManager.Instance.Get(_projectilePrefab.gameObject, _self.position + direction * 0.5f, Quaternion.identity);
                var proj = go.GetComponent<Projectile2D>();
                if (proj != null) proj.Initialize(_spec, _self, _target);
            }

            _spawned = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            IsAttacking = false;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            return Time.time >= _nextAttackTime && Vector3.Distance(_self.position, target.position) <= _range;
        }
    }
}
