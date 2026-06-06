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
            }
            else
            {
            }
        }

        private static void ProcessResource(GameObject go, bool isTree)
        {
            // 1. Standardize Layer (Layer 12: Interactable)
            Undo.RecordObject(go, "Change Layer");
            go.layer = INTERACTABLE_LAYER;

            // 2. Ensure core components
            var node = EnsureComponent<ResourceNode>(go);
            
            if (isTree)
            {
                EnsureComponent<TreeResource>(go);
            }
            else
            {
                // Rocks use RockResource if available
                EnsureComponent<RockResource>(go);
            }

            // 3. Configure via SerializedObject (Safe Editing)
            if (node != null)
            {
                SerializedObject so = new SerializedObject(node);
                
                // A. Configure Health
                SerializedProperty hpProp = so.FindProperty("maxHealth");
                if (hpProp != null)
                {
                    float targetHp = isTree ? TREE_MAX_HEALTH : ROCK_MAX_HEALTH;
                    hpProp.floatValue = targetHp;
                }

                // B. Animator Check (Required for Trees, optional/no-op for Rocks)
                if (isTree)
                {
                    SerializedProperty animProp = so.FindProperty("animator");
                    if (animProp != null && animProp.objectReferenceValue == null)
                    {
                        var animator = go.GetComponent<Animator>() ?? go.GetComponentInChildren<Animator>();
                        if (animator != null)
                        {
                            animProp.objectReferenceValue = animator;
                        }
                    }
                }
                
                so.ApplyModifiedProperties();
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
