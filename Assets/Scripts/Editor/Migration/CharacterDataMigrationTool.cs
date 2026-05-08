#if UNITY_EDITOR
using Gameplay.Characters;
using UnityEditor;
using UnityEngine;

namespace Editor.Migration
{
    public class CharacterDataMigrationTool : EditorWindow
    {
        [MenuItem("Tools/Migrate Character Data")]
        public static void ShowWindow()
        {
            GetWindow<CharacterDataMigrationTool>("Character Data Migration");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Data Migration Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool attempts to recover orphaned serialized data (maxHealth, baseDamage, etc.) from the refactored Character script and apply it to the new SOLID components.", MessageType.Info);

            if (GUILayout.Button("Migrate All Characters in Scene"))
            {
                MigrateAllInScene();
            }
        }

        private void MigrateAllInScene()
        {
            var characters = FindObjectsOfType<Character>(true);
            int count = 0;

            foreach (var character in characters)
            {
                if (MigrateCharacter(character))
                {
                    count++;
                }
            }

            Debug.Log($"[Migration Tool] Successfully migrated {count} characters.");
        }

        private bool MigrateCharacter(Character character)
        {
            SerializedObject serializedCharacter = new SerializedObject(character);
            
            // Try to find orphaned properties
            var maxHealthProp = serializedCharacter.FindProperty("maxHealth");
            var baseDamageProp = serializedCharacter.FindProperty("baseDamage");
            var baseDefenseProp = serializedCharacter.FindProperty("baseDefense");
            var baseMoveSpeedProp = serializedCharacter.FindProperty("baseMoveSpeed");
            var baseAttackCooldownProp = serializedCharacter.FindProperty("baseAttackCooldown");
            var animatorProp = serializedCharacter.FindProperty("animator");

            if (maxHealthProp == null && baseDamageProp == null)
            {
                Debug.LogWarning($"[Migration Tool] No orphaned data found on {character.gameObject.name}. Has it already been migrated and saved?");
                return false;
            }

            Undo.RecordObject(character.gameObject, "Migrate Character Data");

            // 1. Health Migration
            var health = character.GetComponent<CharacterHealth>();
            if (health == null) health = Undo.AddComponent<CharacterHealth>(character.gameObject);
            
            if (health != null)
            {
                SerializedObject serializedHealth = new SerializedObject(health);
                if (maxHealthProp != null) serializedHealth.FindProperty("maxHealth").floatValue = maxHealthProp.floatValue;
                if (baseDefenseProp != null) serializedHealth.FindProperty("baseDefense").floatValue = baseDefenseProp.floatValue;
                serializedHealth.ApplyModifiedProperties();
            }

            // 2. Movement Migration
            var movement = character.GetComponent<PlayerMovementController>();
            if (movement == null) movement = Undo.AddComponent<PlayerMovementController>(character.gameObject);

            if (movement != null)
            {
                SerializedObject serializedMovement = new SerializedObject(movement);
                if (baseMoveSpeedProp != null) serializedMovement.FindProperty("baseMoveSpeed").floatValue = baseMoveSpeedProp.floatValue;
                serializedMovement.ApplyModifiedProperties();
            }

            // 3. Interaction Migration
            var interaction = character.GetComponent<PlayerInteractionController>();
            if (interaction == null && character is Player) interaction = Undo.AddComponent<PlayerInteractionController>(character.gameObject);

            if (interaction != null)
            {
                SerializedObject serializedInteraction = new SerializedObject(interaction);
                if (baseDamageProp != null) serializedInteraction.FindProperty("baseDamage").floatValue = baseDamageProp.floatValue;
                if (baseAttackCooldownProp != null) serializedInteraction.FindProperty("baseAttackCooldown").floatValue = baseAttackCooldownProp.floatValue;
                serializedInteraction.ApplyModifiedProperties();
            }

            // 4. Equipment Animator Migration
            var equipAnimator = character.GetComponent<PlayerEquipmentAnimator>();
            if (equipAnimator == null && character is Player) equipAnimator = Undo.AddComponent<PlayerEquipmentAnimator>(character.gameObject);

            if (equipAnimator != null && animatorProp != null && animatorProp.objectReferenceValue != null)
            {
                SerializedObject serializedEquipAnim = new SerializedObject(equipAnimator);
                serializedEquipAnim.FindProperty("animator").objectReferenceValue = animatorProp.objectReferenceValue;
                serializedEquipAnim.ApplyModifiedProperties();
            }
            
            // Note: Since Character still has animator reference in some ways, we leave it or clean it up.
            // But we can't delete properties from code, Unity will do it when the prefab is saved.

            EditorUtility.SetDirty(character.gameObject);
            Debug.Log($"[Migration Tool] Migrated data for {character.gameObject.name}.");
            return true;
        }
    }
}
#endif
