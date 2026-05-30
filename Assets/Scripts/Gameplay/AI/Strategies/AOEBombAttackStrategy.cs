using Core.Contracts.AI;
using Core.Contracts.Combat;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.Projectiles;
using UnityEngine;
using Infrastructure.Pooling;
using System.Collections.Generic;

namespace Gameplay.AI.Strategies
{
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
            InternalApplyHit();
            if (_animator != null && (_animator.GetCurrentState() != AnimationStateNames.Attack || _animator.IsCurrentAnimationFinished()))
            {
                EndAttack();
            }
        }

        private void InternalApplyHit()
        {
            if (!IsAttacking || _target == null || _animator == null) return;
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

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
            _activeBombs.RemoveAll(b => b == null || !b.activeInHierarchy);

            if (Time.time < _nextAttackTime) return false;
            if (_activeBombs.Count >= _maxActiveBombs) return false;

            return Vector2.Distance(_selfTransform.position, target.position) <= _range;
        }

        private void SpawnBomb()
        {
            // [IMPROVEMENT] Target Prediction for Bomb
            Vector3 predictedPos = _target.position;
            var rb = _target.GetComponent<Rigidbody2D>();
            if (rb != null && _spec != null && _spec.Speed > 0)
            {
                float distance = Vector2.Distance(_selfTransform.position, predictedPos);
                float travelTime = distance / _spec.Speed;
                predictedPos += (Vector3)rb.velocity * travelTime * 0.5f; // Conservative prediction for bombs
            }

            Vector3 direction = (predictedPos - _selfTransform.position).normalized;
            Vector3 mouthOffset = new Vector3(0f, 1.2f, 0f); 
            var spawnPos = _selfTransform.position + mouthOffset + (direction * 0.8f);

            if (_projectilePrefab != null)
            {
                var go = ObjectPoolManager.Instance.Get(_projectilePrefab, spawnPos, Quaternion.identity);
                var proj = go.GetComponent<IProjectile>();
                if (proj != null)
                {
                    // Create temporary target at predicted position
                    GameObject tempTarget = new GameObject("BombTarget");
                    tempTarget.transform.position = predictedPos;
                    
                    proj.Initialize(_spec, _selfTransform, tempTarget.transform);
                    _activeBombs.Add(go);
                    
                    Object.Destroy(tempTarget, 0.1f);
                }
            }
        }
    }
}