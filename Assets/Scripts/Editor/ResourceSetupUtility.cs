using UnityEditor;
using UnityEngine;
using Gameplay.World;
using Gameplay.Environment;
using UnityEditor.SceneManagement;

namespace Editor.Utilities
{
    /// <summary>
    /// Utility script to automate the setup and cleanup of Resource objects (Trees, Rocks) in the scene.
    /// Follows Source-Driven Development:
    /// - Uses Object.FindObjectsByType (Unity 2021.3+)
    /// - Uses Undo system for all modifications
    /// - Uses SerializedObject for safe field editing
    /// </summary>
    public static class ResourceSetupUtility
    {
        private const int INTERACTABLE_LAYER = 12;
        private const float TREE_MAX_HEALTH = 3f;
        private const float ROCK_MAX_HEALTH = 10f;

        [MenuItem("PBL3/Resource/Setup All Resources")]
        public static void SetupAllResources()
        {
            var allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int processedCount = 0;

            foreach (var go in allGameObjects)
            {
                bool isTree = go.name.Contains("Tiny_tree");
                bool isRock = go.name.Contains("Rock");

                if (isTree || isRock)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Setup Resource: " + go.name);
                    int group = Undo.GetCurrentGroup();

                    ProcessResource(go, isTree);
                    processedCount++;
                    
                    Undo.CollapseUndoOperations(group);
                }
            }

            if (processedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log($"<color=green>ResourceSetupUtility:</color> Processed {processedCount} resources successfully.");
            }
            else
            {
                Debug.LogWarning("ResourceSetupUtility: No GameObjects containing 'Tiny_tree' or 'Rock' found.");
            }
        }

        private static void ProcessResource(GameObject go, bool isTree)
        {
            // 1. Standardize Layer
            Undo.RecordObject(go, "Change Layer");
            go.layer = INTERACTABLE_LAYER;

            // 2. Ensure core components
            var node = EnsureComponent<ResourceNode>(go);
            
            if (isTree)
            {
                EnsureComponent<TreeResource>(go);
            }

            // 3. Configure Health via SerializedObject
            if (node != null)
            {
                SerializedObject so = new SerializedObject(node);
                SerializedProperty hpProp = so.FindProperty("maxHealth");
                if (hpProp != null)
                {
                    float targetHp = isTree ? TREE_MAX_HEALTH : ROCK_MAX_HEALTH;
                    // We only set it if it's the default or 0 to avoid overwriting custom values
                    if (hpProp.floatValue <= 0 || hpProp.floatValue == 3f || hpProp.floatValue == 1f) 
                    {
                        hpProp.floatValue = targetHp;
                        so.ApplyModifiedProperties();
                    }
                }
            }
            
            EditorUtility.SetDirty(go);
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = Undo.AddComponent<T>(go);
            }
            return comp;
        }
    }
}
