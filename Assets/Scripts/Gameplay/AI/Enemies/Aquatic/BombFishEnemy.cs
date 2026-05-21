using Core.Contracts.AI;
using Data.Enemies;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies.Aquatic
{
    public class BombFishEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/BombFishConfig";
        private const string AnimationPath = "Enemies/Animations/BombFishAnims";
        private const string ProjectilePath = "Enemies/Projectiles/Bomb";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);
            var projectilePrefab = Resources.Load<Projectile2D>(ProjectilePath);
            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectilePath + "Spec");

            // Strategy: Ranged AOE (Bắn bom nổ lan)
            var attackStrategy = new RangedAOEStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animConfig?.AttackTriggerFrame ?? 3,
                transform,
                config?.BaseAttackRange ?? 9f,
                config?.AttackCooldown ?? 2.0f);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 9f);
        }
    }
}
