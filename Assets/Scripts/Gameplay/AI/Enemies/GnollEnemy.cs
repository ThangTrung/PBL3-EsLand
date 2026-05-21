using Core.Contracts.AI;
using Data.Enemies;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class GnollEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/GnollConfig";
        private const string AnimationPath = "Enemies/Animations/GnollAnims";
        private const string ProjectilePath = "Enemies/Projectiles/BoneShard";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);
            var projectilePrefab = Resources.Load<Projectile2D>(ProjectilePath);
            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectilePath + "Spec");

            // Strategy: Ranged Projectile (Ném xương)
            var attackStrategy = new RangedProjectileAttackStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animConfig?.AttackTriggerFrame ?? 3,
                transform,
                config?.BaseAttackRange ?? 10f,
                config?.AttackCooldown ?? 1.4f);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 10f);
        }
    }
}
