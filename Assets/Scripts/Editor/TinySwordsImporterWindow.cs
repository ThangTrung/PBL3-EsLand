using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TinySwordsImporterWindow : EditorWindow
{
    private string targetPath = "Assets/Textures/Tiny Swords Enemy Pack";

    [MenuItem("Tools/Tiny Swords Importer")]
    public static void ShowWindow()
    {
        GetWindow<TinySwordsImporterWindow>("Tiny Swords Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tiny Swords Sprite Processor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool will process all .png files in the target folder and its subfolders.", MessageType.Info);
        
        targetPath = EditorGUILayout.TextField("Target Path", targetPath);

        if (GUILayout.Button("Process Tiny Swords Sprites"))
        {
            ProcessSprites();
        }
    }

    private void ProcessSprites()
    {
        string folderPath = targetPath.TrimEnd('/');
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", $"Folder not found: {folderPath}\nPlease check the path and try again.", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Only process PNGs as requested
            if (!path.ToLower().EndsWith(".png")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Multiple)
                {
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    changed = true;
                }

                if (importer.spritePixelsPerUnit != 64)
                {
                    importer.spritePixelsPerUnit = 64;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }

                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }

        EditorUtility.DisplayDialog("Success", $"Processed {count} sprites successfully!", "OK");
        
        AssetDatabase.Refresh();
    }
}
