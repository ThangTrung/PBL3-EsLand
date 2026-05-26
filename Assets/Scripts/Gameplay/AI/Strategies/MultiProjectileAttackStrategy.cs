using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Chiến thuật tấn công ném nhiều đạn cùng lúc theo hình nan quạt (Shotgun spread).
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
        
        // Cấu hình nan quạt
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
            if (!IsAttacking || _target == null || _animator == null || _spec == null) return;
            if (_animator.GetCurrentState() != CharacterAnimationController.AnimState.Attack) return;

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
            Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f); // Nâng lên khớp với tay
            Vector3 spawnPos = _selfTransform.position + spawnOffset + (Vector3)baseDirection * 0.5f;

            // Tính toán góc chia đều
            float startAngle = -_spreadAngle / 2f;
            float angleStep = _projectileCount > 1 ? _spreadAngle / (_projectileCount - 1) : 0f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 spreadDirection = RotateVector(baseDirection, currentAngle);

                // Tạo một Transform ảo (ảo để giả lập vị trí đích cho đạn tự tính góc)
                // Projectile2D hiện tại yêu cầu Transform target, ta tạm thời truyền _target 
                // nhưng sẽ phải override hướng bay trong Projectile2D nếu muốn nó bay chéo.
                // Để tương thích với code Projectile2D hiện tại, ta sẽ tạo một GameObject tạm thời.
                
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

                // Truyền điểm đích ảo vào
                projectileInstance.Initialize(_spec, _selfTransform, tempTarget.transform);
                
                // Tự động hủy điểm đích ảo sau 2 giây (khi đạn đã bay xa)
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
