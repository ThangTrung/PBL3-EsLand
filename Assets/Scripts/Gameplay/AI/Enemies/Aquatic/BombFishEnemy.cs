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
        private const string ProjectilePath = "Enemies/Projectiles/Bomb/Bomb";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);
            
            // HOTFIX: Load dưới dạng GameObject thay vì Projectile2D
            var projectilePrefab = Resources.Load<GameObject>(ProjectilePath);
            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectilePath + "Spec");

            // Strategy: AOE Bomb (Bắn bom nổ lan - Giới hạn 2 quả)
            var attackStrategy = new AOEBombAttackStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animConfig?.AttackTriggerFrame ?? 3,
                transform,
                config?.BaseAttackRange ?? 9f,
                config?.AttackCooldown ?? 2.0f,
                2); // Max 2 bombs active

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 9f);
        }
    }
}
