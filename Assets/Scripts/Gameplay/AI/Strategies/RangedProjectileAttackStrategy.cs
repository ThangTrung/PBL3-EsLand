using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using UnityEngine;
using Infrastructure.Pooling;

namespace Gameplay.AI.Strategies
{
    public class RangedProjectileAttackStrategy : IAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly Projectile2D _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly float _cooldown;
        private readonly float _range;

        private Transform _target;
        private bool _projectileSpawned;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public RangedProjectileAttackStrategy(ProjectileSpec spec, Projectile2D projectilePrefab, CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, float range, float cooldown)
        {
            _spec = spec;
            _projectilePrefab = projectilePrefab;
            _animator = animator;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
            _selfTransform = selfTransform;
            _range = range;
            _cooldown = cooldown;
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null || _spec == null) return;

            _target = target;
            _projectileSpawned = false;
            IsAttacking = true;

            _animator.PlayAttack();
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _target == null || _animator == null || _spec == null) return;
            if (_animator.GetCurrentState() != CharacterAnimationController.AnimState.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_projectileSpawned || currentFrame < _attackTriggerFrame) return;

            var distance = Vector3.Distance(_selfTransform.position, _target.position);
            if (distance > _range) return;

            SpawnProjectile();
            _projectileSpawned = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _target = null;
            _projectileSpawned = false;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            if (Time.time < _nextAttackTime) return false;

            var distance = Vector3.Distance(_selfTransform.position, target.position);
            return distance <= _range;
        }

        private void SpawnProjectile()
        {
            var direction = (_target.position - _selfTransform.position).normalized;
            
            // Nâng toạ độ Y lên khớp với tay/miệng (tuỳ thuộc vào Sprite)
            Vector3 spawnOffset = new Vector3(0f, 0.8f, 0f); 
            var spawnPos = _selfTransform.position + spawnOffset + (direction * 0.5f);

            Projectile2D projectileInstance;
            if (_projectilePrefab != null)
            {
                projectileInstance = ObjectPoolManager.Instance.Get(_projectilePrefab.gameObject, spawnPos, Quaternion.identity).GetComponent<Projectile2D>();
            }
            else
            {
                var go = new GameObject("Projectile2D");
                go.transform.position = spawnPos;
                projectileInstance = go.AddComponent<Projectile2D>();
            }

            projectileInstance.Initialize(_spec, _selfTransform, _target);
        }
    }
}
