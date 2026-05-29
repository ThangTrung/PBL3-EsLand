using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class HammerSmashAttackStrategy : IAttackStrategy
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly float _aoeRadius;
        private readonly float _cooldown;
        private readonly CharacterAnimationController _animator;
                private readonly int[] _attackTriggerFrames;
        private readonly System.Collections.Generic.HashSet<int> _triggeredFrames = new System.Collections.Generic.HashSet<int>();
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private Transform _target;

        
        private enum AttackPhase
        {
            None,
            Windup,
            Attack,
            Recovery
        }
        
        private AttackPhase _currentPhase = AttackPhase.None;
private float _nextAttackTime;

                public bool IsAttacking => _currentPhase != AttackPhase.None;

        public HammerSmashAttackStrategy(float damage, float range, float aoeRadius, float cooldown, 
            CharacterAnimationController animator, int[] attackTriggerFrames, Transform selfTransform, Gameplay.Characters.Character source)
        {
            _damage = damage;
            _range = range;
            _aoeRadius = aoeRadius;
            _cooldown = cooldown;
            _animator = animator;
            _attackTriggerFrames = attackTriggerFrames ?? new int[0];
            _selfTransform = selfTransform;
            _source = source;
        }

        // Overload for backward compatibility with single trigger frame
        public HammerSmashAttackStrategy(float damage, float range, float aoeRadius, float cooldown, 
            CharacterAnimationController animator, int attackTriggerFrame, Transform selfTransform, Gameplay.Characters.Character source)
            : this(damage, range, aoeRadius, cooldown, animator, new int[] { attackTriggerFrame }, selfTransform, source)
        {
        }

        public void BeginAttack(Transform target)
        {
            if (_animator == null) return;
            _target = target;
            _triggeredFrames.Clear();
            
            _currentPhase = AttackPhase.Windup;
            _animator.PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Windup);
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _animator == null) return;

            switch (_currentPhase)
            {
                case AttackPhase.Windup:
                    if (_animator.IsCurrentAnimationFinished())
                    {
                        _currentPhase = AttackPhase.Attack;
                        _animator.PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Attack);
                    }
                    break;

                case AttackPhase.Attack:
                    var currentFrame = _animator.GetCurrentFrameIndex();
                    
                    // Check all possible trigger frames
                    foreach (var frame in _attackTriggerFrames)
                    {
                        if (!_triggeredFrames.Contains(frame) && currentFrame == frame)
                        {
                            ApplyAOEDamage();
                            _triggeredFrames.Add(frame);
                            _nextAttackTime = Time.time + _cooldown;
                        }
                    }

                    if (_animator.IsCurrentAnimationFinished())
                    {
                        _currentPhase = AttackPhase.Recovery;
                        _animator.PlayAnimation(Gameplay.AI.Animation.AnimationStateNames.Recovery);
                    }
                    break;

                case AttackPhase.Recovery:
                    if (_animator.IsCurrentAnimationFinished())
                    {
                        EndAttack();
                    }
                    break;
            }
        }

        private void ApplyAOEDamage()
        {
            // Position smash in front of boss
            Vector3 smashPos = _selfTransform.position + (_selfTransform.localScale.x > 0 ? Vector3.right : Vector3.left) * 1.5f;
            
            var colliders = Physics2D.OverlapCircleAll(smashPos, _aoeRadius);
            foreach (var col in colliders)
            {
                if (col.transform == _selfTransform) continue;
                
                if (col.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(_damage, _source);
                }
            }
        }

        public void EndAttack()
        {
            _currentPhase = AttackPhase.None;
            _target = null;
        }

        public bool CanStartAttack(Transform target)
        {
            if (IsAttacking || target == null || Time.time < _nextAttackTime) return false;
            return Vector3.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}
