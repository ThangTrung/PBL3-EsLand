using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class SpiderEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/SpiderConfig";
        private const string AnimationPath = "Enemies/Animations/SpiderAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Poison Melee (Cắn gây độc)
            var attackStrategy = new PoisonMeleeAttackStrategy(
                config?.BaseDamage ?? 5f,
                config?.BaseAttackRange ?? 2.3f,
                config?.AttackCooldown ?? 1.1f,
                Animator,
                animConfig?.AttackTriggerFrame ?? 2,
                transform,
                this,
                0.5f, // Poison DPS
                4f); // Poison Duration

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.3f);
        }
    }
}
