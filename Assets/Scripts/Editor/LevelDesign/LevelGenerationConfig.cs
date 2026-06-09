using UnityEngine;

namespace LevelDesign
{
    [CreateAssetMenu(fileName = "LevelGenerationConfig", menuName = "EsLand/Level Generation Config")]
    public class LevelGenerationConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject fishHutPrefab;
        public GameObject cavePrefab;
        public GameObject goblinHutPrefab;

        [Header("Phase 1: Fish Huts")]
        public Vector2Int fishHutWaterDistance = new Vector2Int(2, 5);
        public int fishHutCount = 3;
        public float fishHutSpacing = 15f;

        [Header("Phase 2: Caves & Goblins")]
        public int landSpawnerInlandDistance = 3;
        public int caveCount = 3;
        public int goblinHutCount = 3;
        public float landSpawnerSpacing = 10f;

        [Header("Phase 3: Boss Arenas")]
        public int bossArenaPadding = 5;
        public GameObject[] bossArenaPrefabs;
    }
}