using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class SpearGoblinEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/SpearGoblinConfig";
        private const string AnimationPath = "Enemies/Animations/SpearGoblinAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Poke and Retreat (Range attack then move back)
            var attackStrategy = new PokeAndRetreatStrategy(
                config?.BaseDamage ?? 6f,
                config?.BaseAttackRange ?? 3.5f,
                config?.AttackCooldown ?? 1.1f,
                2f, // Retreat distance
                Animator,
                GetComponent<EnemyMovementController>(),
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 3.5f);
        }
    }
}
