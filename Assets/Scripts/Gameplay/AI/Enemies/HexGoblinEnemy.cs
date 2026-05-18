using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class HexGoblinEnemy : EnemyBase
    {
        [SerializeField] private HexProjectile hexProjectilePrefab;

        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new CustomHexAttackStrategy(
                hexProjectilePrefab,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                attackRange,
                config.AttackCooldown);

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}