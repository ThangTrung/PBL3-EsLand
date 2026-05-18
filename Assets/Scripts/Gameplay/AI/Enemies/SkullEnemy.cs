using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class SkullEnemy : EnemyBase
    {
        [Header("Skull Specifics")]
        [SerializeField] private float blockChance = 0.15f;

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

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }

        // Note: Shield Block logic should be integrated into CharacterHealth 
        // or a separate component that listens to damage events.
    }
}