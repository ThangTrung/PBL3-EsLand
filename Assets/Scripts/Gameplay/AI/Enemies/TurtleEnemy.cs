using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class TurtleEnemy : EnemyBase
    {
        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new MeleeAttackStrategy(
                config.BaseDamage,
                attackRange,
                config.AttackCooldown,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                this);

            if (GetComponent<ShellDefenseModifier>() == null)
            {
                gameObject.AddComponent<ShellDefenseModifier>();
            }

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }

        public override void ResetStats()
        {
            base.ResetStats();
            // Optional: reset modifier if needed
        }
    }
}