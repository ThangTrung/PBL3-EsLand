using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class PigEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/PigConfig";
        private const string AnimationPath = "Enemies/Animations/PigAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Passive Flee (Doesn't attack, just runs away)
            var attackStrategy = new PassiveFleeStrategy(
                GetComponent<EnemyMovementController>(),
                transform);

            InitializeEnemy(config, animConfig, attackStrategy, 0f);
        }
    }
}
