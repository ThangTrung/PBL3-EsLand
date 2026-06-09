using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace LevelDesign
{
    public interface ILevelGenerationCommand
    {
        void Execute(LevelGenerationConfig config);
    }

    public static class LevelDesignHelpers
    {
        public static bool CheckWaterArea(Tilemap water, Tilemap land, Vector3Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, 0);
                    if (!water.HasTile(pos) || land.HasTile(pos)) return false;
                }
            }
            return true;
        }

        public static bool CheckSolidLand(Tilemap land, Vector3Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (!land.HasTile(center + new Vector3Int(x, y, 0))) return false;
                }
            }
            return true;
        }

        public static bool CheckLandProximity(Tilemap land, Vector3Int center, int minRadius, int maxRadius)
        {
            bool hasLand = false;
            for (int x = -maxRadius; x <= maxRadius; x++)
            {
                for (int y = -maxRadius; y <= maxRadius; y++)
                {
                    if (Mathf.Abs(x) < minRadius && Mathf.Abs(y) < minRadius) continue;
                    if (land.HasTile(center + new Vector3Int(x, y, 0))) hasLand = true;
                }
            }
            return hasLand;
        }

        public static bool CheckWaterProximity(Tilemap water, Tilemap land, Vector3Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, 0);
                    if (water.HasTile(pos) && !land.HasTile(pos)) return true;
                }
            }
            return false;
        }

        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class CleanUpCommand : ILevelGenerationCommand
    {
        public void Execute(LevelGenerationConfig config)
        {
            string[] namesToDelete = { "Environment_Spawners", "Environment_Spawners_Water", "Environment_Spawners_Land", "Boss_Arenas", "Biome_Water", "Biome_Land" };
            int count = 0;
            foreach (string name in namesToDelete)
            {
                GameObject go = GameObject.Find(name);
                if (go != null)
                {
                    Undo.DestroyObjectImmediate(go);
                    count++;
                }
            }

            string[] tagsToDestroy = { "Fish Hut", "Cave", "Goblin Hut", "Arena Trigger" };
            GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in gos)
            {
                if (go.scene.isLoaded)
                {
                    foreach (string tag in tagsToDestroy)
                    {
                        if (go.name.Contains(tag))
                        {
                            Undo.DestroyObjectImmediate(go);
                            count++;
                            break;
                        }
                    }
                }
            }
            Debug.Log($"[Smart Level Designer] Cleaned up {count} objects.");
        }
    }

    public class PlaceFishHutsCommand : ILevelGenerationCommand
    {
        public void Execute(LevelGenerationConfig config)
        {
            if (config.fishHutPrefab == null)
            {
                Debug.LogError("[Smart Level Designer] Fish Hut Prefab is missing in Config!");
                return;
            }

            Tilemap waterMap = GameObject.Find("Water_A")?.GetComponent<Tilemap>();
            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();

            if (waterMap == null || landMap == null) return;

            Transform parent = new GameObject("Environment_Spawners_Water").transform;
            Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Environment Spawners Water");

            BoundsInt bounds = waterMap.cellBounds;
            List<Vector3Int> validSpots = new List<Vector3Int>();

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (waterMap.HasTile(pos) && !landMap.HasTile(pos))
                    {
                        if (LevelDesignHelpers.CheckLandProximity(landMap, pos, config.fishHutWaterDistance.x, config.fishHutWaterDistance.y))
                        {
                            if (LevelDesignHelpers.CheckWaterArea(waterMap, landMap, pos, 1))
                            {
                                validSpots.Add(pos);
                            }
                        }
                    }
                }
            }

            LevelDesignHelpers.Shuffle(validSpots);
            int placed = 0;

            foreach (var spot in validSpots)
            {
                if (placed >= config.fishHutCount) break;

                bool tooClose = false;
                Vector3 worldPos = waterMap.CellToWorld(spot) + new Vector3(0.5f, 0.5f, 0);

                foreach (Transform child in parent)
                {
                    if (Vector3.Distance(child.position, worldPos) < config.fishHutSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    GameObject hut = (GameObject)PrefabUtility.InstantiatePrefab(config.fishHutPrefab);
                    hut.transform.position = worldPos;
                    hut.transform.parent = parent;
                    Undo.RegisterCreatedObjectUndo(hut, "Place Fish Hut");
                    placed++;
                }
            }

            Debug.Log($"[Smart Level Designer] Placed {placed} Fish Huts.");
        }
    }

    public class PlaceLandSpawnersCommand : ILevelGenerationCommand
    {
        public void Execute(LevelGenerationConfig config)
        {
            if (config.cavePrefab == null || config.goblinHutPrefab == null)
            {
                Debug.LogError("[Smart Level Designer] Cave or Goblin Hut Prefab is missing in Config!");
                return;
            }

            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();
            Tilemap waterMap = GameObject.Find("Water_A")?.GetComponent<Tilemap>();

            if (landMap == null || waterMap == null) return;

            Transform parent = new GameObject("Environment_Spawners_Land").transform;
            Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Environment Spawners Land");

            BoundsInt bounds = landMap.cellBounds;
            List<Vector3Int> validSpots = new List<Vector3Int>();

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (landMap.HasTile(pos))
                    {
                        if (!LevelDesignHelpers.CheckWaterProximity(waterMap, landMap, pos, config.landSpawnerInlandDistance))
                        {
                            validSpots.Add(pos);
                        }
                    }
                }
            }

            if (validSpots.Count == 0)
            {
                Debug.LogWarning($"[Smart Level Designer] Could not find any valid spots for Caves/Goblin Huts. Try reducing 'Inland Distance' (currently {config.landSpawnerInlandDistance}) or ensure your island is large enough.");
            }

            LevelDesignHelpers.Shuffle(validSpots);
            int placedCaves = 0;
            int placedGoblins = 0;

            foreach (var spot in validSpots)
            {
                if (placedCaves >= config.caveCount && placedGoblins >= config.goblinHutCount) break;

                Vector3 worldPos = landMap.CellToWorld(spot) + new Vector3(0.5f, 0.5f, 0);
                bool tooClose = false;

                foreach (Transform child in parent)
                {
                    if (Vector3.Distance(child.position, worldPos) < config.landSpawnerSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    GameObject prefabToUse = (placedCaves < config.caveCount) ? config.cavePrefab : config.goblinHutPrefab;
                    GameObject spawner = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
                    spawner.transform.position = worldPos;
                    spawner.transform.parent = parent;
                    Undo.RegisterCreatedObjectUndo(spawner, "Place Land Spawner");

                    if (prefabToUse == config.cavePrefab) placedCaves++;
                    else placedGoblins++;
                }
            }

            Debug.Log($"[Smart Level Designer] Placed {placedCaves} Caves and {placedGoblins} Goblin Huts.");
        }
    }

    public class PlaceBossArenasCommand : ILevelGenerationCommand
    {
        public void Execute(LevelGenerationConfig config)
        {
            if (config.bossArenaPrefabs == null || config.bossArenaPrefabs.Length == 0)
            {
                Debug.LogError("[Smart Level Designer] Boss Arena Prefabs array is empty in Config! Please create and assign your Boss Arena Prefabs.");
                return;
            }

            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();
            if (landMap == null) return;

            Transform parent = new GameObject("Boss_Arenas").transform;
            Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Boss Arenas");

            BoundsInt bounds = landMap.cellBounds;
            
            Vector2[] idealCorners = {
                new Vector2(bounds.xMin, bounds.yMax),
                new Vector2(bounds.xMax, bounds.yMax),
                new Vector2(bounds.xMin, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMin)
            };

            Vector3Int[] bestSpots = new Vector3Int[4];
            float[] minDists = { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };
            bool[] foundSpot = { false, false, false, false };

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (!landMap.HasTile(pos)) continue;

                if (!LevelDesignHelpers.CheckSolidLand(landMap, pos, config.bossArenaPadding)) continue;

                Vector2 cellPos = new Vector2(pos.x, pos.y);
                
                for (int i = 0; i < 4; i++)
                {
                    float dist = Vector2.Distance(cellPos, idealCorners[i]);
                    if (dist < minDists[i])
                    {
                        minDists[i] = dist;
                        bestSpots[i] = pos;
                        foundSpot[i] = true;
                    }
                }
            }

            int placed = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!foundSpot[i]) continue;

                // Pick a prefab from the array (cycle through or clamp if not enough prefabs)
                GameObject prefabToUse = config.bossArenaPrefabs[Mathf.Min(i, config.bossArenaPrefabs.Length - 1)];
                if (prefabToUse == null) continue;

                Vector3 worldPos = landMap.CellToWorld(bestSpots[i]);
                GameObject arena = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
                arena.transform.position = worldPos;
                arena.transform.parent = parent;

                Undo.RegisterCreatedObjectUndo(arena, "Place Boss Arena");
                placed++;
            }

            Debug.Log($"[Smart Level Designer] Placed {placed} Boss Arenas precisely on land using Prefabs.");
        }
    }
}