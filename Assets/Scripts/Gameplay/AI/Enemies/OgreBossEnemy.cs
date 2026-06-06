using Core.Contracts.AI;
using Core.Events;
using Gameplay.AI.Animation;
using Gameplay.AI.Bosses;
using Gameplay.AI.Bosses.Ogre;
using Gameplay.AI.Factories;
using Gameplay.AI.States;
using Gameplay.AI.Strategies;
using Gameplay.Characters;
using UnityEngine;
using Infrastructure.Pooling;
using System.Collections.Generic;

namespace Gameplay.AI.Enemies
{
    public class OgreBossEnemy : EnemyBase
    {
        [Header("Weapon Gibs")]
        [SerializeField] private GameObject clubPart1Prefab;
        [SerializeField] private GameObject clubPart2Prefab;
        
        [Header("Boss Settings")]
        [SerializeField] private string bossName = "Ogre Warlord";
        [SerializeField] private float hammerAOERadius = 2.5f;

        [Header("Minion Spawning")]
        [SerializeField] private GameObject minionPrefab;
        [SerializeField] private ScriptableObject minionConfig;
        [SerializeField] private AnimationConfig minionAnimConfig;
        [SerializeField] private int maxActiveMinions = 6;

        private IBossPhase _currentPhase;
        private List<EnemyBase> _activeMinions = new List<EnemyBase>();
        private bool _isTransitioning;
        private CharacterHealth _cachedHealth;

        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            _cachedHealth = GetComponent<CharacterHealth>();
            if (_cachedHealth != null)
            {
                // Ngăn chặn việc đăng ký sự kiện nhiều lần (Memory leak)
                _cachedHealth.OnHealthChanged -= HandleHealthChanged;
                _cachedHealth.OnDie -= HandleBossDefeated;
                
                _cachedHealth.OnHealthChanged += HandleHealthChanged;
                _cachedHealth.OnDie += HandleBossDefeated;
            }

            _currentPhase = new OgrePhase1();
            _isTransitioning = false;

            var strategy = attackStrategy ?? CreateHammerStrategy(config, attackRange);
            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
            
            // Notify UI
            if (_cachedHealth != null)
            {
                BossHealthEventChannel.RaiseBossHealthUpdated(bossName, _cachedHealth.CurrentHealth, _cachedHealth.MaxHealth);
            }
        }

        public override void ResetStats()
        {
            base.ResetStats();
            _isTransitioning = false;
            _currentPhase = new OgrePhase1();
            ClearMinions();
        }

        protected override void Update()
        {
            base.Update();
            if (!_isTransitioning)
            {
                _currentPhase?.ExecutePhase(this);
            }
        }

        public void SpawnMinions(int count)
        {
            if (minionPrefab == null || minionConfig == null) return;

            // Remove dead minions from list
            _activeMinions.RemoveAll(m => m == null || !m.gameObject.activeInHierarchy);

            int currentCount = _activeMinions.Count;
            int toSpawn = Mathf.Min(count, maxActiveMinions - currentCount);

            for (int i = 0; i < toSpawn; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 3f;
                Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
                
                var minion = EnemyFactory.Instance.CreateEnemy(minionPrefab, (IEnemyConfig)minionConfig, minionAnimConfig, null, spawnPos, transform.parent);
                if (minion != null)
                {
                    _activeMinions.Add(minion);
                }
            }
        }

        public void CompletePhaseTransition()
        {
            if (IsDeadInternal) return;
            
            _isTransitioning = false;
            _currentPhase?.EnterPhase(this);
            ChangeState(CreateChaseState());
        }

        private void HandleHealthChanged(float currentHP)
        {
            BossHealthEventChannel.RaiseBossHealthUpdated(bossName, currentHP, _cachedHealth.MaxHealth);

            if (_isTransitioning) return;

            float hpPercent = currentHP / _cachedHealth.MaxHealth;

            if (_currentPhase is OgrePhase1 && hpPercent <= 0.66f)
            {
                TriggerTransition(new OgrePhase2());
            }
            else if (_currentPhase is OgrePhase2 && hpPercent <= 0.33f)
            {
                TriggerTransition(new OgrePhase3());
            }
        }

        private void TriggerTransition(IBossPhase nextPhase)
        {
            _isTransitioning = true;
            _currentPhase?.ExitPhase(this);
            _currentPhase = nextPhase;
            
            // Apply new strategy based on phase (e.g. faster cooldown or higher damage)
            UpdateBossStrategy(nextPhase);

            ChangeState(new BossTransitionState());
        }

        private void UpdateBossStrategy(IBossPhase phase)
        {
            float cooldownReduction = 1f;
            if (phase is OgrePhase2) cooldownReduction = 0.8f;
            if (phase is OgrePhase3) cooldownReduction = 0.5f;

            var attackSeq = Animator.Config.GetSequence(Gameplay.AI.Animation.AnimationStateNames.Attack);
            int[] triggerFrames = attackSeq?.multiTriggerFrames != null && attackSeq.Value.multiTriggerFrames.Length > 0 
                ? attackSeq.Value.multiTriggerFrames 
                : new int[] { attackSeq?.triggerFrame ?? -1 };

            // Replace strategy with a buffed version
            var newStrategy = new HammerSmashAttackStrategy(
                ConfigInternal.BaseDamage,
                AttackRangeInternal,
                hammerAOERadius,
                ConfigInternal.AttackCooldown * cooldownReduction,
                Animator,
                triggerFrames,
                transform,
                this);
            
            AttackStrategyInternal = newStrategy;
        }

        private IAttackStrategy CreateHammerStrategy(IEnemyConfig config, float range)
        {
            var attackSeq = Animator.Config.GetSequence(Gameplay.AI.Animation.AnimationStateNames.Attack);
            int[] triggerFrames = attackSeq?.multiTriggerFrames != null && attackSeq.Value.multiTriggerFrames.Length > 0 
                ? attackSeq.Value.multiTriggerFrames 
                : new int[] { attackSeq?.triggerFrame ?? -1 };

            return new HammerSmashAttackStrategy(
                config.BaseDamage,
                range,
                hammerAOERadius,
                config.AttackCooldown,
                Animator,
                triggerFrames,
                transform,
                this);
        }

        private void HandleBossDefeated()
        {
            BossHealthEventChannel.RaiseBossDefeated();
            ClearMinions();
            SpawnBrokenClubParts();
        }

        private void SpawnBrokenClubParts()
        {
            if (clubPart1Prefab != null)
            {
                var part1 = ObjectPoolManager.Instance.Get(clubPart1Prefab, transform.position, Quaternion.identity, transform.parent);
                if (part1 != null && part1.TryGetComponent<Rigidbody2D>(out var rb1))
                {
                    rb1.velocity = new Vector2(Random.Range(-3f, -1f), Random.Range(2f, 5f));
                    rb1.angularVelocity = Random.Range(-180f, 180f);
                }
            }

            if (clubPart2Prefab != null)
            {
                var part2 = ObjectPoolManager.Instance.Get(clubPart2Prefab, transform.position, Quaternion.identity, transform.parent);
                if (part2 != null && part2.TryGetComponent<Rigidbody2D>(out var rb2))
                {
                    rb2.velocity = new Vector2(Random.Range(1f, 3f), Random.Range(2f, 5f));
                    rb2.angularVelocity = Random.Range(-180f, 180f);
                }
            }
        }


private void ClearMinions()
        {
            if (_activeMinions == null) return;

            foreach (var minion in _activeMinions)
            {
                if (minion != null && minion.gameObject.activeInHierarchy)
                {
                    // Force immediate return to pool for all minions to prevent leaks
                    ObjectPoolManager.Instance.Return(minion.gameObject);
                }
            }
            _activeMinions.Clear();
        }

        protected override void OnDestroy()
        {
            if (_cachedHealth != null)
            {
                _cachedHealth.OnHealthChanged -= HandleHealthChanged;
                _cachedHealth.OnDie -= HandleBossDefeated;
            }
            base.OnDestroy();
        }
    

        protected override void Awake()
        {
            base.Awake();

            // Fallback: Tự động khởi tạo nếu Boss được đặt thẳng trên Scene (không qua Factory/Spawner)
            if (ConfigInternal == null)
            {
                var config = Resources.Load<Data.Enemies.SimpleEnemyConfig>("Enemies/Configs/OgreBossConfig");
                var animConfig = Resources.Load<AnimationConfig>("Enemies/Animations/OgreBossAnims");

                if (config != null && animConfig != null)
                {
                    InitializeEnemy(config, animConfig, null, config.BaseAttackRange);
                }
            }
        }
}
}