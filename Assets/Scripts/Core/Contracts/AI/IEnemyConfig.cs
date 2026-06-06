using UnityEngine;

namespace Core.Contracts.AI
{
    [System.Serializable]
    public struct EnemyDropConfig
    {
        public Data.Items.ItemData item;
        public int minQuantity;
        public int maxQuantity;
        [Range(0, 1)] public float dropChance;
    }

    public interface IEnemyConfig
    {
        string EnemyType { get; }
        float MaxHealth { get; }
        float BaseDamage { get; }
        float BaseAttackRange { get; }
        float DetectionRange { get; }
        float PatrolRadius { get; }
        float AttackCooldown { get; }

        // Defense Configuration
        float DefenseCooldown { get; }
        float DefenseDuration { get; }
        float DefenseChance { get; }

        float VerticalAlignmentOffset { get; }
        float HorizontalAlignmentOffset { get; }
        float MoveSpeed { get; }
        Color TintColor { get; }
        System.Collections.Generic.List<EnemyDropConfig> LootDrops { get; }

    }
}