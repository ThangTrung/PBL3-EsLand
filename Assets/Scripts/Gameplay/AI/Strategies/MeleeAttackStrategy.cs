using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class MeleeAttackStrategy : BaseAttackStrategy
    {
        private readonly float _damage;
        private readonly int _attackTriggerFrame;

        public MeleeAttackStrategy(float damage, float range, float cooldown, CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source)
            : base(animator, selfTransform, cooldown, range, source)
        {
            _damage = damage;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
        }

        protected override void OnBeginAttack()
        {
            _hitApplied = false;
        }

        protected override void InternalApplyHit()
        {
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (!_hitApplied && currentFrame >= _attackTriggerFrame)
            {
                float verticalOffset = 0f;
                if (_source is Gameplay.AI.EnemyBase enemyBase && enemyBase.Config != null)
                {
                    verticalOffset = enemyBase.Config.VerticalAlignmentOffset;
                }

                float distX = Mathf.Abs(_selfTransform.position.x - _target.position.x);
                float distY = Mathf.Abs(_selfTransform.position.y - (_target.position.y + verticalOffset));
                
                // MỞ RỘNG DUNG SAI ĐỂ TRÁNH ĐÁNH HỤT
                bool isYAligned = distY <= 0.8f; // Cũ: 0.5f
                bool isXInRange = distX <= _range + 0.5f; // Cũ: _range + 0.2f

                if (isYAligned && isXInRange && _target.TryGetComponent<Core.Contracts.Combat.IDamageable>(out var victim))
                {
                    victim.TakeDamage(_damage, _source);
                    Debug.Log($"[Melee] {_source.name} hit {_target.name} for {_damage} damage!");
                }
                else if (isYAligned && isXInRange)
                {
                    Debug.LogWarning($"[Melee] {_source.name} missed {_target.name} - No IDamageable found!");
                }

                _hitApplied = true;
                _nextAttackTime = Time.time + _cooldown;
            }
        }

        protected override void OnEndAttack()
        {
            _hitApplied = false;
        }

        public override bool CanStartAttack(Transform target)
        {
            if (!base.CanStartAttack(target)) return false;

            float verticalOffset = 0f;
            if (_source is Gameplay.AI.EnemyBase enemyBase && enemyBase.Config != null)
            {
                verticalOffset = enemyBase.Config.VerticalAlignmentOffset;
            }

            float distX = Mathf.Abs(_selfTransform.position.x - target.position.x);
            float distY = Mathf.Abs(_selfTransform.position.y - (target.position.y + verticalOffset));
            float totalDist = Vector3.Distance(_selfTransform.position, target.position);
            
            // [FIX] CƯỜNG HÓA TẤN CÔNG: 
            // 1. Nới lỏng dung sai Y lên 1.2m
            // 2. Nếu đứng RẤT gần ( < 1m), tấn công bất chấp độ lệch hàng.
            bool isYAligned = distY <= 1.2f || totalDist < 1.0f;
            bool isXInRange = distX <= _range + 0.3f;

            return isYAligned && isXInRange;
        }
    }
}