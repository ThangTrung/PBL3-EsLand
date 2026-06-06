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

            var distance = Vector2.Distance(_selfTransform.position, _target.position);
            if (distance > _range + 1.0f) return;

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
            // [IMPROVEMENT] Target Prediction (Leading the shot)
            Vector3 targetPos = _target.position;
            var rb = _target.GetComponent<Rigidbody2D>();
            if (rb != null && _spec != null && _spec.Speed > 0)
            {
                float distance = Vector2.Distance(_selfTransform.position, targetPos);
                float travelTime = distance / _spec.Speed;
                targetPos += (Vector3)rb.velocity * travelTime * 0.8f; // 80% prediction factor for fairness
            }

            Vector3 direction = (targetPos - _selfTransform.position).normalized;
            Vector3 spawnOffset = new Vector3(0f, 0.8f, 0f); 
            var spawnPos = _selfTransform.position + spawnOffset + (direction * 0.5f);

            Projectile2D projectileInstance;
            if (_projectilePrefab != null)
            {
                // [FIX] Truyền parent để hỗ trợ kế thừa Elevation Layer cho Projectile
                projectileInstance = ObjectPoolManager.Instance.Get(_projectilePrefab.gameObject, spawnPos, Quaternion.identity, _selfTransform.parent).GetComponent<Projectile2D>();
            }
            else
            {
                var go = new GameObject("Projectile2D");
                go.transform.position = spawnPos;
                projectileInstance = go.AddComponent<Projectile2D>();
            }

            // We need a temporary transform or modify Projectile2D to accept a target position
            // For now, using a temp object is the safest non-breaking way
            GameObject tempTarget = new GameObject("TempTarget");
            tempTarget.transform.position = targetPos;
            
            projectileInstance.Initialize(_spec, _selfTransform, tempTarget.transform);
            Object.Destroy(tempTarget, 1f);
        }
    }
}