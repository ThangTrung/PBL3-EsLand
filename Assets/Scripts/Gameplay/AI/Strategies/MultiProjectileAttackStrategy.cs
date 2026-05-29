using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Chiáº¿n thuáº­t táº¥n cÃ´ng nÃ©m nhiá»u Ä‘áº¡n cÃ¹ng lÃºc theo hÃ¬nh nan quáº¡t (Shotgun spread).
    /// </summary>
    public class MultiProjectileAttackStrategy : IAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly Projectile2D _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly float _cooldown;
        private readonly float _range;
        
        // Cáº¥u hÃ¬nh nan quáº¡t
        private readonly int _projectileCount;
        private readonly float _spreadAngle;

        private Transform _target;
        private bool _projectileSpawned;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public MultiProjectileAttackStrategy(
            ProjectileSpec spec, 
            Projectile2D projectilePrefab, 
            CharacterAnimationController animator, 
            int attackTriggerFrame, 
            Transform selfTransform, 
            float range, 
            float cooldown,
            int projectileCount = 3,
            float spreadAngle = 30f)
        {
            _spec = spec;
            _projectilePrefab = projectilePrefab;
            _animator = animator;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
            _selfTransform = selfTransform;
            _range = range;
            _cooldown = cooldown;
            
            _projectileCount = Mathf.Max(1, projectileCount);
            _spreadAngle = spreadAngle;
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
            InternalApplyHit();
            if (_animator != null && _animator.IsCurrentAnimationFinished())
            {
                EndAttack();
            }
        }

        private void InternalApplyHit()
        {
            if (!IsAttacking || _target == null || _animator == null || _spec == null) return;
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_projectileSpawned || currentFrame < _attackTriggerFrame) return;

            var distance = Vector3.Distance(_selfTransform.position, _target.position);
            if (distance > _range) return;

            SpawnMultipleProjectiles();
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

        private void SpawnMultipleProjectiles()
        {
            if (_target == null) return;

            Vector2 baseDirection = (_target.position - _selfTransform.position).normalized;
            Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f); // NÃ¢ng lÃªn khá»›p vá»›i tay
            Vector3 spawnPos = _selfTransform.position + spawnOffset + (Vector3)baseDirection * 0.5f;

            // TÃ­nh toÃ¡n gÃ³c chia Ä‘á»u
            float startAngle = -_spreadAngle / 2f;
            float angleStep = _projectileCount > 1 ? _spreadAngle / (_projectileCount - 1) : 0f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 spreadDirection = RotateVector(baseDirection, currentAngle);

                // Táº¡o má»™t Transform áº£o (áº£o Ä‘á»ƒ giáº£ láº­p vá»‹ trÃ­ Ä‘Ã­ch cho Ä‘áº¡n tá»± tÃ­nh gÃ³c)
                // Projectile2D hiá»‡n táº¡i yÃªu cáº§u Transform target, ta táº¡m thá»i truyá»n _target 
                // nhÆ°ng sáº½ pháº£i override hÆ°á»›ng bay trong Projectile2D náº¿u muá»‘n nÃ³ bay chÃ©o.
                // Äá»ƒ tÆ°Æ¡ng thÃ­ch vá»›i code Projectile2D hiá»‡n táº¡i, ta sáº½ táº¡o má»™t GameObject táº¡m thá»i.
                
                GameObject tempTarget = new GameObject("TempTarget");
                tempTarget.transform.position = spawnPos + (Vector3)spreadDirection * 10f;

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

                // Truyá»n Ä‘iá»ƒm Ä‘Ã­ch áº£o vÃ o
                projectileInstance.Initialize(_spec, _selfTransform, tempTarget.transform);
                
                // Tá»± Ä‘á»™ng há»§y Ä‘iá»ƒm Ä‘Ã­ch áº£o sau 2 giÃ¢y (khi Ä‘áº¡n Ä‘Ã£ bay xa)
                Object.Destroy(tempTarget, 2f);
            }
        }

        private Vector2 RotateVector(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
    }
}
