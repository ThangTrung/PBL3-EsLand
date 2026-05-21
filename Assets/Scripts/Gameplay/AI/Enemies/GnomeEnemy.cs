using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class GnomeEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/GnomeConfig";
        private const string AnimationPath = "Enemies/Animations/GnomeAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Heavy Blunt (Slow but strong)
            var attackStrategy = new HeavyBluntStrategy(
                config?.BaseDamage ?? 10f,
                config?.BaseAttackRange ?? 2.5f,
                config?.AttackCooldown ?? 1.5f,
                5f, // Knockback force
                Animator,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.5f);
        }
    }
}
