using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class CustomHexAttackStrategy : IAttackStrategy
    {
        private readonly HexProjectile _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly float _range;
        private readonly float _cooldown;

        private Transform _target;
        private bool _projectileSpawned;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public CustomHexAttackStrategy(HexProjectile projectilePrefab, CharacterAnimationController animator, 
            int attackTriggerFrame, Transform selfTransform, float range, float cooldown)
        {
            _projectilePrefab = projectilePrefab;
            _animator = animator;
            _attackTriggerFrame = attackTriggerFrame;
            _selfTransform = selfTransform;
            _range = range;
            _cooldown = cooldown;
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null) return;
            _target = target;
            _projectileSpawned = false;
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
            if (_projectileSpawned || currentFrame < _attackTriggerFrame) return;

            SpawnHexProjectile();
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
            if (IsAttacking || target == null || Time.time < _nextAttackTime) return false;
            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }

        private void SpawnHexProjectile()
        {
            if (_projectilePrefab == null) return;

            var obj = ObjectPoolManager.Instance.Get(_projectilePrefab.gameObject, _selfTransform.position, Quaternion.identity);
            if (obj.TryGetComponent<HexProjectile>(out var proj))
            {
                proj.Initialize(_selfTransform, _target);
            }
        }
    }
}
