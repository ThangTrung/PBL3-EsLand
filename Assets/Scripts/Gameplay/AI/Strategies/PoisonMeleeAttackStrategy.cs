using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using Gameplay.Combat.StatusEffects;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class PoisonMeleeAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
        private readonly int _attackTriggerFrame;
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;
        private readonly float _poisonDps;
        private readonly float _poisonDuration;

        private Transform _target;
        private bool _hitApplied;
        private float _nextAttackTime;

        public bool IsAttacking { get; private set; }

        public PoisonMeleeAttackStrategy(float damage, float range, float cooldown, CharacterAnimationController animator, 
            int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source,
            float poisonDps, float poisonDuration)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
            _animator = animator;
            _attackTriggerFrame = Mathf.Max(0, attackTriggerFrame);
            _selfTransform = selfTransform;
            _source = source;
            _poisonDps = poisonDps;
            _poisonDuration = poisonDuration;
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null) return;

            _target = target;
            _hitApplied = false;
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
            if (!IsAttacking || _target == null || _animator == null) return;
            if (_animator.GetCurrentState() != Gameplay.AI.Animation.AnimationStateNames.Attack) return;

            var currentFrame = _animator.GetCurrentFrameIndex();
            if (_hitApplied || currentFrame < _attackTriggerFrame) return;

            float verticalOffset = 0f;
            if (_source is Gameplay.AI.EnemyBase enemyBase && enemyBase.Config != null)
            {
                verticalOffset = enemyBase.Config.VerticalAlignmentOffset;
            }

            float distX = Mathf.Abs(_selfTransform.position.x - _target.position.x);
            float distY = Mathf.Abs(_selfTransform.position.y - (_target.position.y + verticalOffset));
            bool isYAligned = distY <= 0.25f;
            bool isXInRange = distX <= _range + 0.1f;

            if (isYAligned && isXInRange)
            {
                if (_target.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(_damage, _source);
                }

                if (_target.TryGetComponent<StatusEffectController>(out var statusController))
                {
                    var poison = new PoisonEffect(_poisonDps, _poisonDuration, _source);
                    statusController.ApplyEffect(poison);
                }
            }

            _hitApplied = true;
            _nextAttackTime = Time.time + _cooldown;
        }

        public void EndAttack()
        {
            IsAttacking = false;
            _target = null;
            _hitApplied = false;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null) return false;
            if (Time.time < _nextAttackTime) return false;

            float verticalOffset = 0f;
            if (_source is Gameplay.AI.EnemyBase enemyBase && enemyBase.Config != null)
            {
                verticalOffset = enemyBase.Config.VerticalAlignmentOffset;
            }

            float distX = Mathf.Abs(_selfTransform.position.x - target.position.x);
            float distY = Mathf.Abs(_selfTransform.position.y - (target.position.y + verticalOffset));
            bool isYAligned = distY <= 0.25f;
            bool isXInRange = distX <= _range + 0.1f;

            return isYAligned && isXInRange;
        }
    }
}
