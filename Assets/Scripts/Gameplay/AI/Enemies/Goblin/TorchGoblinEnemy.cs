using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class TorchGoblinEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/TorchGoblinConfig";
        private const string AnimationPath = "Enemies/Animations/TorchGoblinAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Melee with Fire DoT (Simulated via config damage or custom logic)
            // For now, reusing MeleeAttackStrategy as requested
            var attackStrategy = new MeleeAttackStrategy(
                config?.BaseDamage ?? 5f,
                config?.BaseAttackRange ?? 2.5f,
                config?.AttackCooldown ?? 1.0f,
                Animator,
                animConfig?.AttackTriggerFrame ?? 2,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.5f);
        }
    }
}
