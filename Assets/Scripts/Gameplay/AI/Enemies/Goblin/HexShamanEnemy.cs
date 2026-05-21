using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class HexShamanEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/HexShamanConfig";
        private const string AnimationPath = "Enemies/Animations/HexShamanAnims";
        private const string ProjectilePath = "Enemies/Projectiles/HexOrb";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);
            var projectilePrefab = Resources.Load<HexProjectile>(ProjectilePath);

            // Strategy: Custom Hex (Transforms player)
            var attackStrategy = new CustomHexAttackStrategy(
                projectilePrefab,
                Animator,
                animConfig?.AttackTriggerFrame ?? 3,
                transform,
                config?.BaseAttackRange ?? 8f,
                config?.AttackCooldown ?? 2.5f);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 8f);
        }
    }
}
