using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Basic defense strategy that blocks a percentage of damage for a fixed duration.
    /// Used by Minotaur, Skull, etc.
    /// </summary>
    public class StandardDefenseStrategy : IDefenseStrategy
    {
        private float _defenseDuration;
        private float _cooldown;
        private float _nextDefenseTime;
        private float _defenseEndTime;
        private BlockDamageModifier _blockModifier;

        public bool IsDefending { get; private set; }

        public StandardDefenseStrategy(float duration, float cooldown, BlockDamageModifier modifier)
        {
            _defenseDuration = duration;
            _cooldown = cooldown;
            _blockModifier = modifier;
        }

        public bool CanDefend(EnemyBase enemy)
        {
            return !IsDefending && Time.time >= _nextDefenseTime;
        }

        public void BeginDefense(EnemyBase enemy)
        {
            IsDefending = true;
            _defenseEndTime = Time.time + _defenseDuration;
            
            if (_blockModifier != null) _blockModifier.IsActive = true;
            
            enemy.Animator?.PlayAnimation(AnimationStateNames.Defense);
            // Debug.Log($"[Defense] {enemy.name} started defending.");
        }

        public void UpdateDefense(EnemyBase enemy)
        {
            if (Time.time >= _defenseEndTime)
            {
                IsDefending = false;
            }
        }

        public void EndDefense(EnemyBase enemy)
        {
            IsDefending = false;
            if (_blockModifier != null) _blockModifier.IsActive = false;
            _nextDefenseTime = Time.time + _cooldown;
            
            // Go back to Idle animation
            enemy.Animator?.PlayIdle();
            // Debug.Log($"[Defense] {enemy.name} stopped defending.");
        }
    }
}
