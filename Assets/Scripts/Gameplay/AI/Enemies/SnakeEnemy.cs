using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class SnakeEnemy : EnemyBase
    {
        [Header("Snake Specifics")]
        [SerializeField] private float poisonDps = 0.5f;
        [SerializeField] private float poisonDuration = 4f;

        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new PoisonMeleeAttackStrategy(
                config.BaseDamage,
                attackRange,
                config.AttackCooldown,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                this,
                poisonDps,
                poisonDuration);

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}