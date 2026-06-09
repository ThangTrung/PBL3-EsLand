using UnityEngine;
using UnityEditor;

namespace LevelDesign
{
    public class SmartLevelDesignerWindow : EditorWindow
    {
        private LevelGenerationConfig config;

        [MenuItem("Tools/Level Design/Smart Level Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SmartLevelDesignerWindow>("Level Designer");
            window.minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            config = (LevelGenerationConfig)EditorGUILayout.ObjectField("Config Data", config, typeof(LevelGenerationConfig), false);

            if (config == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a Level Generation Config to proceed.", MessageType.Warning);
                if (GUILayout.Button("Create New Config", GUILayout.Height(30)))
                {
                    CreateNewConfig();
                }
                return;
            }

            if (config.fishHutPrefab == null || config.cavePrefab == null || config.goblinHutPrefab == null)
            {
                EditorGUILayout.HelpBox("Some prefabs are missing in the config.", MessageType.Warning);
                if (GUILayout.Button("Auto-Find & Assign Default Prefabs", GUILayout.Height(25)))
                {
                    AutoAssignPrefabs(config);
                }
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(15);
            GUILayout.Label("Phase Execution", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Phase 0: Clean Up", GUILayout.Height(30)))
            {
                ExecuteCommand(new CleanUpCommand());
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Phase 1: Place Fish Huts (Water)", GUILayout.Height(30)))
            {
                ExecuteCommand(new PlaceFishHutsCommand());
            }

            if (GUILayout.Button("Phase 2: Place Caves & Goblin Huts (Land)", GUILayout.Height(30)))
            {
                ExecuteCommand(new PlaceLandSpawnersCommand());
            }

            if (GUILayout.Button("Phase 3: Place Boss Arenas & Decorate", GUILayout.Height(30)))
            {
                ExecuteCommand(new PlaceBossArenasCommand());
            }
        }

        private void ExecuteCommand(ILevelGenerationCommand command)
        {
            if (config == null) return;
            command.Execute(config);
        }

        private void CreateNewConfig()
        {
            LevelGenerationConfig newConfig = ScriptableObject.CreateInstance<LevelGenerationConfig>();
            
            string folderPath = "Assets/Scripts/Editor/LevelDesign";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scripts")) AssetDatabase.CreateFolder("Assets", "Scripts");
                if (!AssetDatabase.IsValidFolder("Assets/Scripts/Editor")) AssetDatabase.CreateFolder("Assets/Scripts", "Editor");
                if (!AssetDatabase.IsValidFolder("Assets/Scripts/Editor/LevelDesign")) AssetDatabase.CreateFolder("Assets/Scripts/Editor", "LevelDesign");
            }

            string assetPath = folderPath + "/DefaultLevelGenConfig.asset";
            AssetDatabase.CreateAsset(newConfig, assetPath);
            AssetDatabase.SaveAssets();
            
            config = newConfig;
            AutoAssignPrefabs(config);
            Debug.Log($"[Smart Level Designer] Created new config at {assetPath}");
        }

        private void AutoAssignPrefabs(LevelGenerationConfig targetConfig)
        {
            if (targetConfig.fishHutPrefab == null)
                targetConfig.fishHutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Enemies/Fish Hut.prefab");
            
            if (targetConfig.cavePrefab == null)
                targetConfig.cavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Enemies/Cave.prefab");
            
            if (targetConfig.goblinHutPrefab == null)
                targetConfig.goblinHutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Enemies/Goblin Hut.prefab");

            EditorUtility.SetDirty(targetConfig);
            AssetDatabase.SaveAssets();
            Debug.Log("[Smart Level Designer] Auto-assigned default prefabs to the config.");
        }
    }
}