using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using UnityEngine;
using System.Collections.Generic;

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
        private readonly HashSet<int> _triggeredFrames = new HashSet<int>();
        private readonly Transform _selfTransform;
        private readonly Gameplay.Characters.Character _source;

        private Transform _target;
        private float _nextAttackTime;

        private enum AttackPhase
        {
            None,
            Windup,
            Attack,
            Recovery
        }
        
        private AttackPhase _currentPhase = AttackPhase.None;

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
            
            // [IMPROVEMENT] Check if Windup exists, otherwise skip to Attack
            if (HasAnimation(AnimationStateNames.Windup))
            {
                _currentPhase = AttackPhase.Windup;
                _animator.PlayAnimation(AnimationStateNames.Windup);
            }
            else
            {
                _currentPhase = AttackPhase.Attack;
                _animator.PlayAttack();
            }
        }

        public void TryApplyHitIfReady()
        {
            if (!IsAttacking || _animator == null) return;

            switch (_currentPhase)
            {
                case AttackPhase.Windup:
                    if (_animator.IsCurrentAnimationFinished() || _animator.GetCurrentState() != AnimationStateNames.Windup)
                    {
                        _currentPhase = AttackPhase.Attack;
                        _animator.PlayAttack();
                    }
                    break;

                case AttackPhase.Attack:
                    if (_animator.GetCurrentState() != AnimationStateNames.Attack) return;

                    var currentFrame = _animator.GetCurrentFrameIndex();
                    foreach (var frame in _attackTriggerFrames)
                    {
                        if (!_triggeredFrames.Contains(frame) && currentFrame >= frame)
                        {
                            ApplyAOEDamage();
                            _triggeredFrames.Add(frame);
                            _nextAttackTime = Time.time + _cooldown;
                        }
                    }

                    if (_animator.IsCurrentAnimationFinished())
                    {
                        if (HasAnimation(AnimationStateNames.Recovery))
                        {
                            _currentPhase = AttackPhase.Recovery;
                            _animator.PlayAnimation(AnimationStateNames.Recovery);
                        }
                        else
                        {
                            EndAttack();
                        }
                    }
                    break;

                case AttackPhase.Recovery:
                    if (_animator.IsCurrentAnimationFinished() || _animator.GetCurrentState() != AnimationStateNames.Recovery)
                    {
                        EndAttack();
                    }
                    break;
            }
        }

        private bool HasAnimation(string stateName)
        {
            if (_animator == null || _animator.Config == null) return false;
            return _animator.Config.GetSequence(stateName).HasValue;
        }

        private void ApplyAOEDamage()
        {
            Vector3 smashPos = _selfTransform.position + (_selfTransform.localScale.x > 0 ? Vector3.right : Vector3.left) * 1.5f;
            
            int envMask = LayerMask.GetMask("Resource", "Environment_Block", "Interactable", "Default");
            bool isEnemy = _source != null && _source.CompareTag("Enemy");

            var colliders = Physics2D.OverlapCircleAll(smashPos, _aoeRadius);
            foreach (var col in colliders)
            {
                if (col.transform.root == _selfTransform.root) continue;
                
                if (isEnemy && ((1 << col.gameObject.layer) & envMask) != 0) continue;

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
            
            // Use 2D distance for fairness
            return Vector2.Distance(_selfTransform.position, target.position) <= _range;
        }
    }
}