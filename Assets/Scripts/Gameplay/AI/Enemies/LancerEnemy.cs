using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class LancerEnemy : EnemyBase
    {
        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            // Lancer uses long reach melee attack
            var strategy = attackStrategy ?? new MeleeAttackStrategy(
                config.BaseDamage,
                attackRange,
                config.AttackCooldown,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                this);

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}