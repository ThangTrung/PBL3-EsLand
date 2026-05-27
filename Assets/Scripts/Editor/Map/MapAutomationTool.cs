using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Editor.Map
{
    public class MapAutomationTool : EditorWindow
    {
        [MenuItem("PBL3 Tools/Automate Map Configuration")]
        public static void AutomateMapConfig()
        {
            // 1. Kiểm tra và tạo PhysicsMaterial2D không ma sát
            string materialPath = "Assets/ZeroFriction.physicsMaterial2D";
            PhysicsMaterial2D zeroFrictionMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
            
            if (zeroFrictionMaterial == null)
            {
                zeroFrictionMaterial = new PhysicsMaterial2D("ZeroFriction");
                zeroFrictionMaterial.friction = 0f;
                zeroFrictionMaterial.bounciness = 0f;
                AssetDatabase.CreateAsset(zeroFrictionMaterial, materialPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created new PhysicsMaterial2D at {materialPath}");
            }

            // 2. Quét toàn bộ Tilemap trong Scene hiện tại
            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>(true);
            int collidersUpdated = 0;

            foreach (Tilemap tm in tilemaps)
            {
                string nameLower = tm.gameObject.name.ToLower();
                
                if (nameLower.Contains("moutain") || nameLower.Contains("mountain") || 
                    nameLower.Contains("water") || nameLower.Contains("props"))
                {
                    TilemapCollider2D tmCollider = tm.gameObject.GetComponent<TilemapCollider2D>();
                    if (tmCollider == null)
                    {
                        tmCollider = tm.gameObject.AddComponent<TilemapCollider2D>();
                    }
                    tmCollider.usedByComposite = true;

                    CompositeCollider2D compCollider = tm.gameObject.GetComponent<CompositeCollider2D>();
                    if (compCollider == null)
                    {
                        compCollider = tm.gameObject.AddComponent<CompositeCollider2D>();
                    }

                    Rigidbody2D rb = tm.gameObject.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Static;
                        rb.sharedMaterial = zeroFrictionMaterial;
                    }

                    collidersUpdated++;
                }
            }

            // 3. Quét toàn bộ TilemapRenderer và đổi mode sang Individual cho Y-Sorting
            TilemapRenderer[] renderers = FindObjectsOfType<TilemapRenderer>(true);
            int renderersUpdated = 0;

            foreach (TilemapRenderer renderer in renderers)
            {
                if (renderer.mode != TilemapRenderer.Mode.Individual)
                {
                    renderer.mode = TilemapRenderer.Mode.Individual;
                    renderersUpdated++;
                }
            }

            // 4. Lưu Scene và hiển thị thông báo
            Scene activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            
            Debug.Log($"<color=green>Map Automation Complete!</color>\n" +
                      $"- Processed Colliders for {collidersUpdated} Tilemaps.\n" +
                      $"- Updated {renderersUpdated} TilemapRenderers to Individual mode.");
        }

        [MenuItem("PBL3 Tools/Auto Assign Elevation Layers")]
        public static void AutoAssignElevationLayers()
        {
            int tier0Layer = LayerMask.NameToLayer("Tier_0");
            int tier1Layer = LayerMask.NameToLayer("Tier_1");
            int tier2Layer = LayerMask.NameToLayer("Tier_2");

            if (tier0Layer == -1 || tier1Layer == -1 || tier2Layer == -1)
            {
                Debug.LogError("Missing required Unity Layers. Please create 'Tier_0', 'Tier_1', and 'Tier_2' layers in the Tags and Layers manager before running this tool.");
                return;
            }

            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            int tier0Count = 0, tier1Count = 0, tier2Count = 0;

            foreach (GameObject go in allObjects)
            {
                string nameLower = go.name.ToLower();
                
                // Bỏ qua cầu thang
                if (nameLower.Contains("stairs"))
                    continue;

                int targetLayer = -1;

                // Phân loại Tier 0
                if (nameLower.Contains("water") || nameLower.Contains("land_1") || nameLower.Contains("shadow_1") || nameLower.Contains("moutain_1") || nameLower.Contains("mountain_1") || nameLower.Contains("props_land"))
                {
                    targetLayer = tier0Layer;
                }
                // Phân loại Tier 1
                else if (nameLower.Contains("shadow_2") || nameLower.Contains("moutain_2") || nameLower.Contains("mountain_2") || nameLower.Contains("props_moutain_1") || nameLower.Contains("props_mountain_1"))
                {
                    targetLayer = tier1Layer;
                }
                // Phân loại Tier 2
                else if (nameLower.Contains("moutain_3") || nameLower.Contains("mountain_3") || nameLower.Contains("props_moutain_2") || nameLower.Contains("props_mountain_2"))
                {
                    targetLayer = tier2Layer;
                }

                // Nếu có layer đích, tiến hành gán đệ quy
                if (targetLayer != -1)
                {
                    int changedCount = SetLayerRecursively(go, targetLayer);
                    
                    if (targetLayer == tier0Layer) tier0Count += changedCount;
                    else if (targetLayer == tier1Layer) tier1Count += changedCount;
                    else if (targetLayer == tier2Layer) tier2Count += changedCount;
                }
            }

            Scene activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            
            Debug.Log($"<color=cyan>Elevation Layers Assigned Successfully!</color>\n" +
                      $"- Tier_0: {tier0Count} objects\n" +
                      $"- Tier_1: {tier1Count} objects\n" +
                      $"- Tier_2: {tier2Count} objects");
        }

        [MenuItem("PBL3 Tools/Auto Fix Order In Layer")]
        public static void AutoFixOrderInLayer()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            int updatedCount = 0;

            foreach (GameObject go in allObjects)
            {
                string nameLower = go.name.ToLower();
                int targetOrder = -1;

                if (nameLower.Contains("water")) 
                    targetOrder = 0;
                else if (nameLower.Contains("land_1") || nameLower.Contains("shadow_1")) 
                    targetOrder = 1;
                else if (nameLower.Contains("moutain_1") || nameLower.Contains("mountain_1") || nameLower.Contains("props_land")) 
                    targetOrder = 2;
                else if (nameLower.Contains("shadow_2")) 
                    targetOrder = 3;
                else if (nameLower.Contains("moutain_2") || nameLower.Contains("mountain_2") || nameLower.Contains("props_moutain_1") || nameLower.Contains("props_mountain_1")) 
                    targetOrder = 4;
                else if (nameLower.Contains("moutain_3") || nameLower.Contains("mountain_3") || nameLower.Contains("props_moutain_2") || nameLower.Contains("props_mountain_2")) 
                    targetOrder = 6;

                if (targetOrder != -1)
                {
                    updatedCount += SetOrderInLayerRecursively(go, targetOrder);
                }
            }

            // Lưu thay đổi cho Scene hiện tại
            Scene activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            
            Debug.Log($"<color=orange>Order In Layer Fixed Successfully!</color>\n" +
                      $"- Updated {updatedCount} Renderers.");
        }


        private static int SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return 0;
            
            int count = 0;
            if (obj.layer != newLayer)
            {
                obj.layer = newLayer;
                count++;
            }

            // Đệ quy gán cho toàn bộ con bên trong
            foreach (Transform child in obj.transform)
            {
                count += SetLayerRecursively(child.gameObject, newLayer);
            }

            return count;
        }

        private static int SetOrderInLayerRecursively(GameObject obj, int newOrder)
        {
            if (obj == null) return 0;
            
            int count = 0;
            Renderer renderer = obj.GetComponent<Renderer>();
            
            if (renderer != null && (renderer is SpriteRenderer || renderer is TilemapRenderer))
            {
                if (renderer.sortingOrder != newOrder)
                {
                    renderer.sortingOrder = newOrder;
                    count++;
                }
            }

            foreach (Transform child in obj.transform)
            {
                count += SetOrderInLayerRecursively(child.gameObject, newOrder);
            }

            return count;
        }

        [MenuItem("PBL3 Tools/Auto Setup Stairs")]
        public static void AutoSetupStairs()
        {
            int updatedCount = 0;

            // Setup Stairs 2
            GameObject stairs2 = GameObject.Find("Stairs_2_layer");
            if (stairs2 != null)
            {
                SetupStairObject(stairs2, "Tier_0", 2, "Tier_1", 4);
                updatedCount++;
            }

            // Setup Stairs 3
            GameObject stairs3 = GameObject.Find("Stairs_3_layer");
            if (stairs3 != null)
            {
                SetupStairObject(stairs3, "Tier_1", 4, "Tier_2", 6);
                updatedCount++;
            }

            if (updatedCount > 0)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                Debug.Log($"<color=yellow>Auto Setup Stairs Completed!</color>\n- Processed {updatedCount} stairs.");
            }
            else
            {
                Debug.LogWarning("No stairs found. Please make sure GameObjects are named 'Stairs_2_layer' and 'Stairs_3_layer'.");
            }
        }

        private static void SetupStairObject(GameObject stairObj, string lowerLayer, int lowerOrder, string upperLayer, int upperOrder)
        {
            // TilemapCollider2D
            TilemapCollider2D tmCollider = stairObj.GetComponent<TilemapCollider2D>();
            if (tmCollider == null)
            {
                tmCollider = stairObj.AddComponent<TilemapCollider2D>();
            }
            tmCollider.isTrigger = true;
            tmCollider.usedByComposite = true;

            // CompositeCollider2D
            CompositeCollider2D compCollider = stairObj.GetComponent<CompositeCollider2D>();
            if (compCollider == null)
            {
                compCollider = stairObj.AddComponent<CompositeCollider2D>();
            }
            compCollider.isTrigger = true;

            // Rigidbody2D
            Rigidbody2D rb = stairObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }

            // SmartStairTilemap script
            global::Map.SmartStairTilemap stairScript = stairObj.GetComponent<global::Map.SmartStairTilemap>();
            if (stairScript == null)
            {
                stairScript = stairObj.AddComponent<global::Map.SmartStairTilemap>();
            }
            stairScript.lowerTierLayer = lowerLayer;
            stairScript.lowerOrder = lowerOrder;
            stairScript.upperTierLayer = upperLayer;
            stairScript.upperOrder = upperOrder;
        }



    }
}
