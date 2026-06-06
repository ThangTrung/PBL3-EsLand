using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace LevelDesign
{
    public class SmartLevelDesigner : EditorWindow
    {
        private GameObject fishHutPrefab;
        private GameObject cavePrefab;
        private GameObject goblinHutPrefab;
        private GameObject rootTrollDecorationGroup; // Optional prefab or folder

        [MenuItem("Tools/Level Design/Smart Level Designer")]
        public static void ShowWindow()
        {
            GetWindow<SmartLevelDesigner>("Smart Level Designer");
        }

        private void OnGUI()
        {
            GUILayout.Label("References", EditorStyles.boldLabel);
            fishHutPrefab = (GameObject)EditorGUILayout.ObjectField("Fish Hut Prefab", fishHutPrefab, typeof(GameObject), false);
            cavePrefab = (GameObject)EditorGUILayout.ObjectField("Cave Prefab", cavePrefab, typeof(GameObject), false);
            goblinHutPrefab = (GameObject)EditorGUILayout.ObjectField("Goblin Hut Prefab", goblinHutPrefab, typeof(GameObject), false);
            
            GUILayout.Space(10);
            GUILayout.Label("Phase Execution", EditorStyles.boldLabel);

            if (GUILayout.Button("Phase 0: Clean Up"))
            {
                CleanUp();
            }

            if (GUILayout.Button("Phase 1: Place Fish Huts (Water)"))
            {
                PlaceFishHuts();
            }

            if (GUILayout.Button("Phase 2: Place Caves & Goblin Huts (Land)"))
            {
                PlaceLandSpawners();
            }

            if (GUILayout.Button("Phase 3: Place Boss Arenas & Decorate"))
            {
                PlaceBossArenas();
            }
        }

        private void CleanUp()
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

        private void PlaceFishHuts()
        {
            if (fishHutPrefab == null)
            {
                fishHutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Enemies/Fish Hut.prefab");
                if (fishHutPrefab == null)
                {
                    Debug.LogError("Fish Hut Prefab is missing!");
                    return;
                }
            }

            Tilemap waterMap = GameObject.Find("Water_A")?.GetComponent<Tilemap>();
            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();

            if (waterMap == null || landMap == null)
            {
                Debug.LogError("Water_A or Land_A tilemap not found.");
                return;
            }

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
                        if (CheckArea(waterMap, landMap, pos, 2, true) && CheckLandProximity(landMap, pos, 10, 15))
                        {
                            validSpots.Add(pos);
                        }
                    }
                }
            }

            // Shuffle and place 2-3 huts
            Shuffle(validSpots);
            int toPlace = Random.Range(2, 4);
            int placed = 0;

            foreach (var spot in validSpots)
            {
                if (placed >= toPlace) break;

                // Ensure distance from other huts
                bool tooClose = false;
                foreach (Transform child in parent)
                {
                    if (Vector3.Distance(child.position, waterMap.CellToWorld(spot)) < 20f)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    GameObject hut = (GameObject)PrefabUtility.InstantiatePrefab(fishHutPrefab);
                    hut.transform.position = waterMap.CellToWorld(spot) + new Vector3(0.5f, 0.5f, 0);
                    hut.transform.parent = parent;
                    Undo.RegisterCreatedObjectUndo(hut, "Place Fish Hut");
                    placed++;
                }
            }

            Debug.Log($"[Smart Level Designer] Placed {placed} Fish Huts.");
        }

        private void PlaceLandSpawners()
        {
            if (cavePrefab == null || goblinHutPrefab == null)
            {
                Debug.LogError("Cave or Goblin Hut Prefab is missing!");
                return;
            }

            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();
            Tilemap waterMap = GameObject.Find("Water_A")?.GetComponent<Tilemap>();

            if (landMap == null || waterMap == null)
            {
                Debug.LogError("Water_A or Land_A tilemap not found.");
                return;
            }

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
                        // Inland check: far from water
                        if (!CheckWaterProximity(waterMap, pos, 20))
                        {
                            validSpots.Add(pos);
                        }
                    }
                }
            }

            Shuffle(validSpots);
            int cavesToPlace = 3;
            int goblinsToPlace = 3;
            int placedCaves = 0;
            int placedGoblins = 0;

            foreach (var spot in validSpots)
            {
                if (placedCaves >= cavesToPlace && placedGoblins >= goblinsToPlace) break;

                Vector3 worldPos = landMap.CellToWorld(spot) + new Vector3(0.5f, 0.5f, 0);

                // Check distance
                bool tooClose = false;
                foreach (Transform child in parent)
                {
                    if (Vector3.Distance(child.position, worldPos) < 30f)
                    {
                        tooClose = true;
                        break;
                    }
                }

                // Physics overlap check
                Collider2D hit = Physics2D.OverlapCircle(worldPos, 2f);
                if (hit != null) tooClose = true;

                if (!tooClose)
                {
                    GameObject prefabToUse = (placedCaves < cavesToPlace) ? cavePrefab : goblinHutPrefab;
                    GameObject spawner = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
                    spawner.transform.position = worldPos;
                    spawner.transform.parent = parent;
                    Undo.RegisterCreatedObjectUndo(spawner, "Place Land Spawner");

                    if (prefabToUse == cavePrefab) placedCaves++;
                    else placedGoblins++;
                }
            }

            Debug.Log($"[Smart Level Designer] Placed {placedCaves} Caves and {placedGoblins} Goblin Huts.");
        }

        private void PlaceBossArenas()
        {
            Tilemap landMap = GameObject.Find("Land_A")?.GetComponent<Tilemap>();
            if (landMap == null) return;

            Transform parent = new GameObject("Boss_Arenas").transform;
            Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Boss Arenas");

            BoundsInt bounds = landMap.cellBounds;
            
            // 4 corners approximation
            Vector3[] corners = {
                landMap.CellToWorld(new Vector3Int(bounds.xMin + 15, bounds.yMax - 15, 0)),
                landMap.CellToWorld(new Vector3Int(bounds.xMax - 15, bounds.yMax - 15, 0)),
                landMap.CellToWorld(new Vector3Int(bounds.xMin + 15, bounds.yMin + 15, 0)),
                landMap.CellToWorld(new Vector3Int(bounds.xMax - 15, bounds.yMin + 15, 0))
            };

            string[] bossNames = { "Turtle Boss", "Minotaur Boss", "Thief Boss", "Ogre Boss" };

            for (int i = 0; i < corners.Length; i++)
            {
                GameObject arena = new GameObject(bossNames[i] + " Arena Trigger");
                arena.transform.position = corners[i];
                arena.transform.parent = parent;

                var col = arena.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(30, 30);

                var spawner = arena.AddComponent<Gameplay.Spawning.BossArenaSpawner>();
                var saveable = arena.AddComponent<Infrastructure.SaveSystem.Core.SaveableEntity>();
                saveable.GenerateGuid();

                if (bossNames[i] == "Ogre Boss")
                {
                    DecorateOgreBossArena(arena.transform);
                }

                Undo.RegisterCreatedObjectUndo(arena, "Place Boss Arena");
            }

            Debug.Log("[Smart Level Designer] Placed 4 Boss Arenas.");
        }

        private void DecorateOgreBossArena(Transform arenaTransform)
        {
            // Decorate with Root Troll sprites
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] {"Assets/Textures/Tiny Swords Enemy Pack/Enemy Pack/Enemies/Root Troll"});
            if (guids.Length == 0) return;

            Transform decorParent = new GameObject("Decorations").transform;
            decorParent.parent = arenaTransform;
            decorParent.localPosition = Vector3.zero;

            for (int i = 0; i < 10; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(10f, 15f);
                Vector3 decorPos = arenaTransform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

                GameObject decor = new GameObject("RootTroll_Decor");
                decor.transform.position = decorPos;
                decor.transform.parent = decorParent;
                decor.layer = LayerMask.NameToLayer("Ignore Raycast"); // or Environment_Block depending on need

                SpriteRenderer sr = decor.AddComponent<SpriteRenderer>();
                string path = AssetDatabase.GUIDToAssetPath(guids[Random.Range(0, guids.Length)]);
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                sr.sortingLayerName = "Props";
                sr.sortingOrder = 1;
            }
        }

        // Helper Methods
        private bool CheckArea(Tilemap water, Tilemap land, Vector3Int center, int radius, bool requireWater)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, 0);
                    if (requireWater && (!water.HasTile(pos) || land.HasTile(pos))) return false;
                }
            }
            return true;
        }

        private bool CheckLandProximity(Tilemap land, Vector3Int center, int minRadius, int maxRadius)
        {
            bool hasLand = false;
            for (int x = -maxRadius; x <= maxRadius; x++)
            {
                for (int y = -maxRadius; y <= maxRadius; y++)
                {
                    if (Mathf.Abs(x) < minRadius && Mathf.Abs(y) < minRadius) continue;
                    Vector3Int pos = center + new Vector3Int(x, y, 0);
                    if (land.HasTile(pos)) hasLand = true;
                }
            }
            return hasLand;
        }

        private bool CheckWaterProximity(Tilemap water, Vector3Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, 0);
                    if (water.HasTile(pos)) return true;
                }
            }
            return false;
        }

        private void Shuffle<T>(List<T> list)
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
}