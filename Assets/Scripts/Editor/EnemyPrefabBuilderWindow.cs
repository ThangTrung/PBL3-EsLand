using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameplay.AI.Animation;
using Data.Enemies;
using Gameplay.AI.Movement;
using Gameplay.Characters;
using Gameplay.Combat.StatusEffects;

public class EnemyPrefabBuilderWindow : EditorWindow
{
    private string rootFolderPath = "Assets/Textures/Tiny Swords Enemy Pack/Enemy Pack/Enemies";
    private string[] actionKeywords = { "Idle", "Run", "Walk", "Attack", "Die" };

    [MenuItem("Tools/Enemy Prefab Builder (Batch)")]
    public static void ShowWindow()
    {
        GetWindow<EnemyPrefabBuilderWindow>("Enemy Prefab Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Enemy Prefab Builder (V2 Refactored)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        rootFolderPath = EditorGUILayout.TextField("Root Folder Path", rootFolderPath);

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("BUILD V2 ENEMIES", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirm V2 Build", "Hành động này sẽ tạo/cập nhật Config, Anims trong Resources và tạo Prefab chuẩn v2.", "Tiến hành", "Hủy"))
            {
                RunBatchBuild();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void RunBatchBuild()
    {
        string systemRootPath = Path.Combine(Application.dataPath, rootFolderPath.Replace("Assets/", ""));
        if (!Directory.Exists(systemRootPath))
        {
            Debug.LogError($"[BatchBuilder] Thư mục gốc không tồn tại: {systemRootPath}");
            return;
        }

        string[] allSubDirs = Directory.GetDirectories(systemRootPath, "*", SearchOption.AllDirectories);
        List<string> enemyFolders = new List<string>();

        foreach (string dir in allSubDirs)
        {
            string assetDir = "Assets" + dir.Replace(Application.dataPath, "").Replace('\\', '/');
            string[] pngFiles = Directory.GetFiles(dir, "*.png");
            bool hasActions = pngFiles.Any(f => actionKeywords.Any(k => f.Contains(k)));
            if (hasActions) enemyFolders.Add(assetDir);
        }

        if (enemyFolders.Count == 0)
        {
            Debug.LogWarning("[BatchBuilder] Không tìm thấy thư mục Enemy nào phù hợp.");
            return;
        }

        int total = enemyFolders.Count;
        int successCount = 0;

        try
        {
            EnsureFolder("Assets/Resources/Enemies/Animations");
            EnsureFolder("Assets/Resources/Enemies/Configs");

            for (int i = 0; i < total; i++)
            {
                string folder = enemyFolders[i];
                string enemyName = Path.GetFileName(folder).Replace(" ", "");

                if (EditorUtility.DisplayCancelableProgressBar("Đang Build Quái vật v2...", $"Xử lý: {enemyName} ({i + 1}/{total})", (float)i / total))
                {
                    Debug.Log("[BatchBuilder] Đã hủy bởi người dùng.");
                    break;
                }

                try
                {
                    ProcessSingleEnemy(folder, enemyName);
                    successCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BatchBuilder] Lỗi khi xử lý {enemyName}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Hoàn tất v2", $"Đã tạo thành công {successCount}/{total} quái vật theo chuẩn mới.", "OK");
        }
    }

    private void ProcessSingleEnemy(string sourcePath, string enemyName)
    {
        string prefabFolderPath = "Assets/Prefabs/Characters/Enemies";
        string prefabPath = $"{prefabFolderPath}/{enemyName}.prefab";
        string resourcesAnimPath = $"Assets/Resources/Enemies/Animations/{enemyName}Anims.asset";
        string resourcesConfigPath = $"Assets/Resources/Enemies/Configs/{enemyName}Config.asset";

        EnsureFolder(prefabFolderPath);
        AutoSliceSprites(sourcePath);

        Dictionary<string, List<Sprite>> spriteGroups = new Dictionary<string, List<Sprite>>();
        foreach (var keyword in actionKeywords) spriteGroups[keyword] = new List<Sprite>();

        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { sourcePath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    foreach (var keyword in actionKeywords)
                    {
                        if (sprite.name.Contains(keyword)) { spriteGroups[keyword].Add(sprite); break; }
                    }
                }
            }
        }

        foreach (var keyword in actionKeywords)
            spriteGroups[keyword] = spriteGroups[keyword].OrderBy(s => s.name).ToList();

        // 1. ScriptableObjects Setup
        AnimationConfig animConfig = AssetDatabase.LoadAssetAtPath<AnimationConfig>(resourcesAnimPath);
        if (animConfig == null)
        {
            animConfig = ScriptableObject.CreateInstance<AnimationConfig>();
            AssetDatabase.CreateAsset(animConfig, resourcesAnimPath);
        }
        var runFrames = spriteGroups["Run"].Count > 0 ? spriteGroups["Run"] : spriteGroups["Walk"];
        animConfig.Initialize(spriteGroups["Idle"].ToArray(), runFrames.ToArray(), spriteGroups["Attack"].ToArray(), spriteGroups["Die"].ToArray(), 12f, 2);
        EditorUtility.SetDirty(animConfig);

        SimpleEnemyConfig enemyConfig = AssetDatabase.LoadAssetAtPath<SimpleEnemyConfig>(resourcesConfigPath);
        if (enemyConfig == null)
        {
            enemyConfig = ScriptableObject.CreateInstance<SimpleEnemyConfig>();
            AssetDatabase.CreateAsset(enemyConfig, resourcesConfigPath);
            enemyConfig.Initialize(enemyName, 25f, 5f, 2.5f, 12f, 5f, 1.2f, 2.8f, Color.white);
        }
        EditorUtility.SetDirty(enemyConfig);

        // 2. GameObject Setup
        GameObject go = new GameObject(enemyName);
        go.tag = "Enemy";
        go.layer = 10;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (spriteGroups["Idle"].Count > 0) sr.sprite = spriteGroups["Idle"][0];

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        go.AddComponent<CharacterHealth>();
        go.AddComponent<EnemyMovementController>();
        var animCtrl = go.AddComponent<CharacterAnimationController>();
        animCtrl.SetConfig(animConfig);
        go.AddComponent<StatusEffectController>();

        // Collider Setup (Polygon for precision)
        go.AddComponent<PolygonCollider2D>();

        Type scriptType = FindType($"{enemyName}Enemy");
        if (scriptType != null) go.AddComponent(scriptType);

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
    }

    private void AutoSliceSprites(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
        var factory = new SpriteDataProviderFactories();
        factory.Init();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().EndsWith(".png")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) continue;

            if (texture.width % texture.height != 0 || texture.width <= texture.height)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                continue;
            }

            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            int cellSize = texture.height;
            int frameCount = texture.width / texture.height;
            List<SpriteRect> rects = new List<SpriteRect>();
            string fileName = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < frameCount; i++)
            {
                rects.Add(new SpriteRect
                {
                    name = $"{fileName}_{i}",
                    rect = new Rect(i * cellSize, 0, cellSize, cellSize),
                    alignment = SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f)
                });
            }

            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            importer.SaveAndReimport();
        }
    }

    private void EnsureFolder(string path)
    {
        string[] folders = path.Split('/');
        string currentPath = folders[0];
        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(nextPath)) AssetDatabase.CreateFolder(currentPath, folders[i]);
            currentPath = nextPath;
        }
    }

    private Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type type = assembly.GetType(typeName) ?? assembly.GetTypes().FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
                if (type != null) return type;
            }
            catch { }
        }
        return null;
    }
}