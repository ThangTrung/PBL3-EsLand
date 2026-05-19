using Core.Contracts.AI;
using Data.Combat;
using Data.Enemies;
using Gameplay.AI.Animation;using Gameplay.AI.Movement;

using Gameplay.AI.States;
using Gameplay.AI.Strategies;
using Gameplay.Characters;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies.Aquatic
{
    public class HarpoonFishEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/HarpoonFishConfig";
        private const string AnimationPath = "Enemies/Animations/HarpoonFishAnims";
        private const string ProjectileSpecPath = "Combat/Projectiles/HarpoonSpec";
        private const string ProjectilePrefabPath = "Combat/Projectiles/Projectile2D";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[HarpoonFishEnemy] Missing config at Resources/{ConfigPath}. Using fallback defaults.");
                config = ScriptableObject.CreateInstance<SimpleEnemyConfig>();
                config.Initialize("HarpoonFish", 24f, 6f, 12f, 14f, 4f, 1.6f, 2.1f, Color.white);
            }

            var animationConfig = Resources.Load<AnimationConfig>(AnimationPath);
            if (animationConfig == null)
            {
                Debug.LogWarning($"[HarpoonFishEnemy] Missing animation config at Resources/{AnimationPath}. Create one to enable animations.");
                animationConfig = ScriptableObject.CreateInstance<AnimationConfig>();
                animationConfig.Initialize(null, null, null, null, 10f, 2);
            }

            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectileSpecPath);
            if (projectileSpec == null)
            {
                Debug.LogWarning($"[HarpoonFishEnemy] Missing projectile spec at Resources/{ProjectileSpecPath}. Using fallback defaults.");
                projectileSpec = ScriptableObject.CreateInstance<ProjectileSpec>();
                projectileSpec.Initialize(config.BaseDamage, 10f, 3f, 0.2f);
            }

            var projectilePrefab = Resources.Load<Projectile2D>(ProjectilePrefabPath);
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[HarpoonFishEnemy] Missing projectile prefab at Resources/{ProjectilePrefabPath}. Using runtime projectile.");
            }

            var health = GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.SetMaxHealth(config.MaxHealth, true);
            }

            var movementController = GetComponent<EnemyMovementController>();
            if (movementController != null)
            {
                movementController.SetBaseMoveSpeed(config.MoveSpeed);
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = config.TintColor;
            }

            var attackStrategy = new RangedProjectileAttackStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                config.BaseAttackRange,
                config.AttackCooldown);

            InitializeEnemy(config, animationConfig, attackStrategy, config.BaseAttackRange);
        }

        public override IAIState CreateChaseState()
        {
            return new KeepDistanceState();
        }
    }
}
