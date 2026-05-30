using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    /// <summary>
    /// Defense strategy for Turtle.
    /// Hides in shell (Invulnerable) and deals AOE damage when popping out.
    /// </summary>
    public class TurtleCounterDefenseStrategy : IDefenseStrategy
    {
        private float _defenseDuration;
        private float _cooldown;
        private float _counterDamage;
        private float _counterRadius;
        private float _nextDefenseTime;
        private float _defenseEndTime;
        private BlockDamageModifier _blockModifier;

        public bool IsDefending { get; private set; }

        public TurtleCounterDefenseStrategy(float duration, float cooldown, float counterDamage, float counterRadius, BlockDamageModifier modifier)
        {
            _defenseDuration = duration;
            _cooldown = cooldown;
            _counterDamage = counterDamage;
            _counterRadius = counterRadius;
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
            
            // Turtle hide in shell is usually 100% block
            if (_blockModifier != null)
            {
                _blockModifier.SetMultiplier(0f); // 100% reduction
                _blockModifier.IsActive = true;
            }
            
            enemy.Animator?.PlayAnimation(AnimationStateNames.Defense);
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

            // Trigger AOE Counter Attack
            ExplodeCounter(enemy);
            
            enemy.Animator?.PlayIdle();
        }

        private void ExplodeCounter(EnemyBase enemy)
        {
            var colliders = Physics2D.OverlapCircleAll(enemy.transform.position, _counterRadius);
            foreach (var col in colliders)
            {
                if (col.transform.root == enemy.transform.root) continue;

                if (col.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(_counterDamage, enemy);
                    // Debug.Log($"[Turtle] Counter hit {col.name} for {_counterDamage}!");
                }
            }
        }
    }
}
