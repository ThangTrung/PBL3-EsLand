using System;
using UnityEngine;
using Data.Enemies;
using Gameplay.AI.Animation;

namespace Data.Spawning
{
    /// <summary>
    /// Chứa thông tin cấu hình cho một loại quái vật cụ thể trong hệ thống Spawner.
    /// </summary>
    [Serializable]
    public class EnemySpawnData
    {
        [Header("Prefab & Config")]
        [Tooltip("Prefab của quái vật (Phải có script kế thừa EnemyBase)")]
        public GameObject prefab;
        
        [Tooltip("Config chỉ số của quái")]
        public SimpleEnemyConfig config;
        
        [Tooltip("Config animation của quái")]
        public AnimationConfig animConfig;

        [Header("Spawn Settings")]
        [Range(1, 100)]
        [Tooltip("Trọng số xuất hiện (Weight). Số càng cao quái xuất hiện càng nhiều trong đợt.")]
        public int weight = 10;

        [Tooltip("Chi phí tài nguyên để sinh quái này. Quái càng mạnh thì Cost nên càng cao.")]
        public float cost = 1.0f;
    }
}
