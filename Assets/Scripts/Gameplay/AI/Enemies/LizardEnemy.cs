using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class LizardEnemy : EnemyBase
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

            if (GetComponent<SpikeCounterModifier>() == null)
            {
                gameObject.AddComponent<SpikeCounterModifier>();
            }

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}