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
                _cachedHealth.OnHealthChanged += HandleHealthChanged;
                _cachedHealth.OnDie += HandleBossDefeated;
            }

            _currentPhase = new OgrePhase1();
            _isTransitioning = false;

            var strategy = attackStrategy ?? CreateHammerStrategy(config, attackRange);
            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
            
            // Notify UI
            BossHealthEventChannel.RaiseBossHealthUpdated(bossName, _cachedHealth.CurrentHealth, _cachedHealth.MaxHealth);
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
                
                var minion = EnemyFactory.Instance.CreateEnemy(minionPrefab, (IEnemyConfig)minionConfig, minionAnimConfig, null, spawnPos);
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

            // Replace strategy with a buffed version
            var newStrategy = new HammerSmashAttackStrategy(
                ConfigInternal.BaseDamage,
                AttackRangeInternal,
                hammerAOERadius,
                ConfigInternal.AttackCooldown * cooldownReduction,
                Animator,
                Animator.Config.AttackTriggerFrame,
                transform,
                this);
            
            // Note: In real logic, we'd need a SetAttackStrategy in EnemyBase or re-initialize
            // For now, we manually overwrite internal member if possible, or re-init
            AttackStrategyInternal = newStrategy;
        }

        private IAttackStrategy CreateHammerStrategy(IEnemyConfig config, float range)
        {
            return new HammerSmashAttackStrategy(
                config.BaseDamage,
                range,
                hammerAOERadius,
                config.AttackCooldown,
                Animator,
                Animator.Config.AttackTriggerFrame,
                transform,
                this);
        }

        private void HandleBossDefeated()
        {
            BossHealthEventChannel.RaiseBossDefeated();
            ClearMinions();
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
    }
}