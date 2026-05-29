using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;using Gameplay.AI.Movement;

using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Chiến thuật húc (Charge): Quái lao thẳng vào mục tiêu.
    /// Đã sửa: Không cộng dồn transform.position trực tiếp để tránh húc xuyên tường.
    /// </summary>
    public class ChargeAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
        private readonly Transform _selfTransform;
        private readonly Character _source;
        private readonly float _chargeSpeedMultiplier = 2.5f;

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
            _animator?.PlayRun(); 
        }

        public void TryApplyHitIfReady()
        {
            if (!_isCharging || _target == null) return;

            // [FIX] SỬ DỤNG RIGIDBODY (VẬT LÝ): Lao bằng lệnh Move để bị vật cản chặn lại.
            // KHÔNG dùng cộng dồn Transform.position += ... (gây xuyên tường và lỗi NavMesh).
            var movementController = _source.GetComponent<EnemyMovementController>();
            if (movementController != null)
            {
                float chargeSpeed = movementController.GetCurrentMoveSpeed() * _chargeSpeedMultiplier;
                movementController.Move(_chargeDirection, chargeSpeed);
            }

            // Kiểm tra va chạm để gây sát thương
            if (Vector3.Distance(_selfTransform.position, _target.position) <= 0.6f)
            {
                ApplyHit();
            }
        }

        private void ApplyHit()
        {
            if (_target.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(_damage, _source);
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
            return dist <= _range && dist > 1.5f;
        }
    }
}