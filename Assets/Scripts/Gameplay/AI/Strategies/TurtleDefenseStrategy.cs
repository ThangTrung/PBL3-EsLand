using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class TurtleDefenseStrategy : IDefenseStrategy
    {
        private float _defenseDuration;
        private float _cooldown;
        private float _nextDefenseTime;
        private float _defenseEndTime;
        private BlockDamageModifier _blockModifier;
        
        private enum DefensePhase { Entering, Holding, Exiting, Finished }
        private DefensePhase _phase = DefensePhase.Finished;

        public bool IsDefending => _phase != DefensePhase.Finished;

        public TurtleDefenseStrategy(float duration, float cooldown, BlockDamageModifier modifier)
        {
            _defenseDuration = duration;
            _cooldown = cooldown;
            _blockModifier = modifier;
        }

        public bool CanDefend(EnemyBase enemy)
        {
            return _phase == DefensePhase.Finished && Time.time >= _nextDefenseTime;
        }

        public void BeginDefense(EnemyBase enemy)
        {
            _phase = DefensePhase.Entering;
            _defenseEndTime = Time.time + _defenseDuration;
            
            if (_blockModifier != null)
            {
                _blockModifier.SetMultiplier(0f); // 100% block
                _blockModifier.IsActive = true;
            }
            
            enemy.Animator?.PlayAnimation("Defense_1");
        }

        public void UpdateDefense(EnemyBase enemy)
        {
            if (enemy.Animator == null) return;

            switch (_phase)
            {
                case DefensePhase.Entering:
                    if (enemy.Animator.IsCurrentAnimationFinished())
                    {
                        _phase = DefensePhase.Holding;
                    }
                    break;

                case DefensePhase.Holding:
                    if (Time.time >= _defenseEndTime)
                    {
                        _phase = DefensePhase.Exiting;
                        enemy.Animator.PlayAnimation("Defense_2");
                    }
                    break;

                case DefensePhase.Exiting:
                    if (enemy.Animator.IsCurrentAnimationFinished())
                    {
                        _phase = DefensePhase.Finished;
                    }
                    break;
            }
        }

        public void EndDefense(EnemyBase enemy)
        {
            _phase = DefensePhase.Finished;
            if (_blockModifier != null) _blockModifier.IsActive = false;
            _nextDefenseTime = Time.time + _cooldown;
            enemy.Animator?.PlayIdle();
        }
    }
}