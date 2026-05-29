using Core.Contracts.AI;
using Data.Enemies;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies.Aquatic
{
    public class HarpoonSharkEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/HarpoonSharkConfig";
        private const string AnimationPath = "Enemies/Animations/HarpoonSharkAnims";
        private const string ProjectilePath = "Enemies/Projectiles/HarpoonShark/Harpoon";

        protected override void Awake()
        {
            // Call base.Awake first to initialize shared components like Animator (AnimationController)
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);
            var projectilePrefab = Resources.Load<Projectile2D>(ProjectilePath);
            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectilePath + "Spec");

            if (config == null || animConfig == null || projectilePrefab == null || projectileSpec == null)
            {
                Debug.LogError($"[HarpoonShark] Failed to load one or more resources! Path: {ProjectilePath}");
                return;
            }

            // Strategy: Ranged Projectile (Ném lao)
            // Pass AnimationController (assigned in base.Awake) to the strategy
            var attackStrategy = new RangedProjectileAttackStrategy(
                projectileSpec,
                projectilePrefab,
                AnimationController, 
                animConfig.AttackTriggerFrame,
                transform,
                config.BaseAttackRange,
                config.AttackCooldown);

            InitializeEnemy(config, animConfig, attackStrategy, config.BaseAttackRange);
        }
    }
}