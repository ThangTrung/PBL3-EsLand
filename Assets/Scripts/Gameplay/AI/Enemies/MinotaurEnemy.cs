using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class MinotaurEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/MinotaurConfig";
        private const string AnimationPath = "Enemies/Animations/MinotaurAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Hammer Smash (AOE)
            var attackStrategy = new HammerSmashAttackStrategy(
                config?.BaseDamage ?? 12f,
                config?.BaseAttackRange ?? 3.0f,
                2.5f, // AOE Radius
                config?.AttackCooldown ?? 1.8f,
                Animator,
                animConfig?.AttackTriggerFrame ?? 4,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 3.0f);
        }
    }
}
