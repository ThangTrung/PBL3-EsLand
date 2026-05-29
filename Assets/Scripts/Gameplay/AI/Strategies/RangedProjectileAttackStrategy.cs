using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using UnityEngine;
using Infrastructure.Pooling;

namespace Gameplay.AI.Strategies
{
    public class RangedProjectileAttackStrategy : BaseAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly Projectile2D _projectilePrefab;
        private readonly int _attackTriggerFrame;

        private bool _projectileSpawned;

        public RangedProjectileAttackStrategy(ProjectileSpec spec, Projectile2D projectilePrefab, CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, float range, float cooldown)
            : base(animator, selfTransform, cooldown, range, null)
        {
            _spec = spec;
            _projectilePrefab = projectilePrefab;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
        }

        protected override void OnBeginAttack()
        {
            _projectileSpawned = false;
        }

        protected override void InternalApplyHit()
        {
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_projectileSpawned || currentFrame < _attackTriggerFrame) return;

            var distance = Vector3.Distance(_selfTransform.position, _target.position);
            if (distance > _range) return;

            SpawnProjectile();
            _projectileSpawned = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        protected override void OnEndAttack()
        {
            _projectileSpawned = false;
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