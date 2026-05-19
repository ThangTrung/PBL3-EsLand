using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;using Gameplay.AI.Movement;

using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class ChargeAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
        private readonly Transform _selfTransform;
        private readonly Character _source;
        private readonly float _chargeSpeedMultiplier = 2.5f;
        private readonly float _knockbackForce = 0.8f;

        private Transform _target;
        private bool _isCharging;
        private float _nextAttackTime;
        private Vector3 _chargeDirection;

        public bool IsAttacking => _isCharging;

        public ChargeAttackStrategy(float damage, float range, float cooldown, CharacterAnimationController animator, 
            Transform selfTransform, Character source)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _animator = animator;
            _selfTransform = selfTransform;
            _source = source;
        }

        public void BeginAttack(Transform target)
        {
            _target = target;
            _isCharging = true;
            _chargeDirection = (_target.position - _selfTransform.position).normalized;
            _animator?.PlayRun(); // Charge uses run animation or special charge if available
            Debug.Log($"{_selfTransform.name} is CHARGING!");
        }

        public void TryApplyHitIfReady()
        {
            if (!_isCharging || _target == null) return;

            // Move during charge
            var movementController = _source.GetComponent<EnemyMovementController>();
            float speed = movementController != null ? movementController.GetCurrentMoveSpeed() : 3f;
            _selfTransform.position += _chargeDirection * (speed * _chargeSpeedMultiplier) * Time.deltaTime;

            // Check collision
            if (Vector3.Distance(_selfTransform.position, _target.position) <= 0.5f)
            {
                ApplyHit();
            }
        }

        private void ApplyHit()
        {
            if (_target.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(_damage, _source);
                // Apply knockback logic here if supported by target
            }
            EndAttack();
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            _isCharging = false;
            _target = null;
            _animator?.PlayIdle();
        }

        public bool CanStartAttack(Transform target)
        {
            if (_isCharging || target == null || Time.time < _nextAttackTime) return false;
            float dist = Vector3.Distance(_selfTransform.position, target.position);
            return dist <= _range && dist > 1.5f; // Charge only if somewhat far
        }
    }
}