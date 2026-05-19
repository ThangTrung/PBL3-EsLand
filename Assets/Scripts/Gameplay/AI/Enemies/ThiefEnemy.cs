using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.AI.Strategies.Modifiers;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class ThiefEnemy : EnemyBase
    {
        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new AssassinAttackStrategy(
                config.BaseDamage,
                attackRange,
                config.AttackCooldown,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                this);

            if (GetComponent<DodgeModifier>() == null)
            {
                var dodge = gameObject.AddComponent<DodgeModifier>();
                dodge.SetDodgeChance(0.2f);
            }

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}