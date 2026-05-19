using Core.Contracts.AI;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class ShamanEnemy : EnemyBase
    {
        [Header("Shaman Specifics")]
        [SerializeField] private ProjectileSpec projectileSpec;
        [SerializeField] private Projectile2D projectilePrefab;

        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new RangedProjectileAttackStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                attackRange,
                config.AttackCooldown);

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }
    }
}