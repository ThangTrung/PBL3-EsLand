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
        Debug.Log("Processing: " + enemyName);

        // 1. Gather Sprites
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { sourcePath });
        Dictionary<string, List<Sprite>> spriteGroups = new Dictionary<string, List<Sprite>>()
        {
            { "Idle", new List<Sprite>() },
            { "Run", new List<Sprite>() },
            { "Walk", new List<Sprite>() },
            { "Attack", new List<Sprite>() },
            { "Die", new List<Sprite>() }
        };

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            string lowerName = sprite.name.ToLower();

            if (lowerName.Contains("idle")) spriteGroups["Idle"].Add(sprite);
            else if (lowerName.Contains("run")) spriteGroups["Run"].Add(sprite);
            else if (lowerName.Contains("walk")) spriteGroups["Walk"].Add(sprite);
            else if (lowerName.Contains("attack")) spriteGroups["Attack"].Add(sprite);
            else if (lowerName.Contains("die") || lowerName.Contains("death")) spriteGroups["Die"].Add(sprite);
        }

        if (spriteGroups["Idle"].Count == 0)
        {
            Debug.LogWarning("Skipping " + enemyName + " - No Idle sprites found.");
            return;
        }

        // 2. Create Configs
        string resourcesConfigPath = $"Assets/Resources/Enemies/Configs/{enemyName}Config.asset";
        string resourcesAnimPath = $"Assets/Resources/Enemies/Animations/{enemyName}Anims.asset";
        EnsureFolder("Assets/Resources/Enemies/Configs");
        EnsureFolder("Assets/Resources/Enemies/Animations");

        AnimationConfig animConfig = AssetDatabase.LoadAssetAtPath<AnimationConfig>(resourcesAnimPath);
        if (animConfig == null)
        {
            animConfig = ScriptableObject.CreateInstance<AnimationConfig>();
            AssetDatabase.CreateAsset(animConfig, resourcesAnimPath);
        }
        var runFrames = spriteGroups["Run"].Count > 0 ? spriteGroups["Run"] : spriteGroups["Walk"];
        
        var seqs = new System.Collections.Generic.List<Gameplay.AI.Animation.AnimationSequence>
        {
            new Gameplay.AI.Animation.AnimationSequence { stateName = "Idle", frames = spriteGroups["Idle"].ToArray(), isLooping = true, triggerFrame = -1 },
            new Gameplay.AI.Animation.AnimationSequence { stateName = "Run", frames = runFrames.ToArray(), isLooping = true, triggerFrame = -1 },
            new Gameplay.AI.Animation.AnimationSequence { stateName = "Attack", frames = spriteGroups["Attack"].ToArray(), isLooping = false, triggerFrame = 2 },
            new Gameplay.AI.Animation.AnimationSequence { stateName = "Death", frames = spriteGroups["Die"].ToArray(), isLooping = false, triggerFrame = -1 }
        };
        animConfig.Initialize(seqs, 12f);
        EditorUtility.SetDirty(animConfig);

        SimpleEnemyConfig enemyConfig = AssetDatabase.LoadAssetAtPath<SimpleEnemyConfig>(resourcesConfigPath);
        if (enemyConfig == null)
        {
            enemyConfig = ScriptableObject.CreateInstance<SimpleEnemyConfig>();
            AssetDatabase.CreateAsset(enemyConfig, resourcesConfigPath);
            enemyConfig.Initialize(enemyName, 25f, 5f, 2.5f, 12f, 5f, 1.2f, 2.8f, Color.white);
        }
        EditorUtility.SetDirty(enemyConfig);

        // 3. Create Prefab
        string prefabPath = $"Assets/Prefabs/Enemies/{enemyName}.prefab";
        EnsureFolder("Assets/Prefabs/Enemies");

        GameObject go = new GameObject(enemyName);
        go.tag = "Enemy";
        go.layer = 10; // Enemy

        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteGroups["Idle"][0];
        spriteRenderer.sortingOrder = 2;

        var animator = go.AddComponent<Gameplay.AI.Animation.CharacterAnimationController>();
        animator.SetConfig(animConfig);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.6f, 1f);
        collider.offset = new Vector2(0, -0.2f);

        go.AddComponent<Gameplay.Characters.CharacterHealth>();
        go.AddComponent<Gameplay.AI.Movement.EnemyMovementController>();
        go.AddComponent<Gameplay.Combat.ContactDamage>();
        go.AddComponent<Gameplay.Characters.Enemy>();

        // Hitbox
        GameObject hitbox = new GameObject("Hitbox");
        hitbox.transform.SetParent(go.transform);
        hitbox.transform.localPosition = Vector3.zero;
        hitbox.layer = 12; // EnemyHitbox
        var hitCollider = hitbox.AddComponent<CapsuleCollider2D>();
        hitCollider.isTrigger = true;
        hitCollider.size = new Vector2(0.8f, 1.2f);

        // Shadow
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(go.transform);
        shadow.transform.localPosition = new Vector3(0, -0.6f, 0);
        var shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = Resources.Load<Sprite>("Shadows/DropShadow");
        shadowRenderer.sortingOrder = 1;
        shadowRenderer.color = new Color(0, 0, 0, 0.4f);

        PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);
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
