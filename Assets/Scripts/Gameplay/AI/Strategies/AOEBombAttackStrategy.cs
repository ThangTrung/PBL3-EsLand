using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using UnityEngine;
using Core.Contracts.Combat;
using Infrastructure.Pooling;
using System.Collections.Generic;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Chiến thuật ném bom diện rộng với giới hạn số lượng bom hoạt động.
    /// Theo blueprint: Max 2 bombs active.
    /// </summary>
    public class AOEBombAttackStrategy : IAttackStrategy
    {
        private readonly ProjectileSpec _spec;
        private readonly GameObject _projectilePrefab;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly int _maxActiveBombs;

        private Transform _target;
        private bool _projectileSpawned;
        private float _nextAttackTime;
        private List<GameObject> _activeBombs = new List<GameObject>();

        public bool IsAttacking { get; private set; }

        public AOEBombAttackStrategy(ProjectileSpec spec, GameObject projectilePrefab, CharacterAnimationController animator, 
            int attackTriggerFrame, Transform selfTransform, float range, float cooldown, int maxActiveBombs = 2)
        {
            _spec = spec;
            _projectilePrefab = projectilePrefab;
            _animator = animator;
            _attackTriggerFrame = attackTriggerFrame;
            _selfTransform = selfTransform;
            _range = range;
            _cooldown = cooldown;
            _maxActiveBombs = maxActiveBombs;
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

            SpawnBomb();
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
            
            // Dọn dẹp danh sách bom đã nổ (trở về pool)
            _activeBombs.RemoveAll(b => b == null || !b.activeInHierarchy);

            // Kiểm tra cooldown và giới hạn số bom
            if (Time.time < _nextAttackTime) return false;
            if (_activeBombs.Count >= _maxActiveBombs) return false;

            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }

        private void SpawnBomb()
        {
            var direction = (_target.position - _selfTransform.position).normalized;
            
            // Nâng toạ độ Y lên một chút để khớp với miệng con cá (bạn có thể thay đổi số 1.2f thành số bạn thấy vừa mắt)
            Vector3 mouthOffset = new Vector3(0f, 1.2f, 0f); 
            var spawnPos = _selfTransform.position + mouthOffset + (direction * 0.8f);

            if (_projectilePrefab != null)
            {
                var go = ObjectPoolManager.Instance.Get(_projectilePrefab, spawnPos, Quaternion.identity);
                var proj = go.GetComponent<IProjectile>();
                if (proj != null)
                {
                    proj.Initialize(_spec, _selfTransform, _target);
                    _activeBombs.Add(go);
                }
            }
        }
    }
}
