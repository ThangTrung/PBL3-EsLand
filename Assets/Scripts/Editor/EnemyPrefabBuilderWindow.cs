// VERSION: ULTIMATE BATCH BUILDER v1.3 (AUTO-SLICING & CLEAN BUILD)
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

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
        GUILayout.Label("Batch Enemy Prefab Builder (Ultimate Version)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        rootFolderPath = EditorGUILayout.TextField("Root Folder Path", rootFolderPath);

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("BATCH BUILD ALL ENEMIES", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirm Batch Build", "Hành động này sẽ quét toàn bộ thư mục con, TỰ ĐỘNG CẮT ẢNH và tạo Prefab hàng loạt.", "Tiến hành", "Hủy"))
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
            for (int i = 0; i < total; i++)
            {
                string folder = enemyFolders[i];
                string enemyName = Path.GetFileName(folder).Replace(" ", "");

                if (EditorUtility.DisplayCancelableProgressBar("Đang Build Quái vật...", $"Xử lý: {enemyName} ({i + 1}/{total})", (float)i / total))
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
            EditorUtility.DisplayDialog("Hoàn tất", $"Đã tạo thành công {successCount}/{total} quái vật.", "OK");
        }
    }

    private void ProcessSingleEnemy(string sourcePath, string enemyName)
    {
        // --- BƯỚC 1: XÁC ĐỊNH ĐƯỜNG DẪN ---
        string animFolderPath = $"Assets/Animations/Characters/Enemies/{enemyName}";
        string prefabFolderPath = "Assets/Prefabs/Characters/Enemies";
        string prefabPath = $"{prefabFolderPath}/{enemyName}.prefab";

        // --- BƯỚC 2: NUKE CLEAN BUILD (Xóa tận gốc) ---
        if (AssetDatabase.IsValidFolder(animFolderPath))
        {
            AssetDatabase.DeleteAsset(animFolderPath);
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        // --- BƯỚC 3: ĐẢM BẢO THƯ MỤC TỒN TẠI ---
        EnsureFolder(animFolderPath);
        EnsureFolder(prefabFolderPath);

        // --- BƯỚC 4: AUTO-SLICING ---
        AutoSliceSprites(sourcePath);

        // --- TIẾP TỤC LOGIC GHÉP NỐI ---
        Dictionary<string, List<Sprite>> spriteGroups = new Dictionary<string, List<Sprite>>();
        foreach (var keyword in actionKeywords) spriteGroups[keyword] = new List<Sprite>();

        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { sourcePath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
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

        if (spriteGroups["Idle"].Count == 0 && spriteGroups.Values.All(v => v.Count == 0))
        {
            throw new Exception("Không tìm thấy Sprite nào sau khi cắt ảnh.");
        }

        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
        foreach (var action in actionKeywords)
        {
            if (spriteGroups[action].Count > 0)
                clips[action] = CreateAnimationClip(spriteGroups[action], action, enemyName, animFolderPath);
        }

        AnimatorController controller = CreateAnimatorController(clips, enemyName, animFolderPath);

        GameObject go = new GameObject(enemyName);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (spriteGroups["Idle"].Count > 0) sr.sprite = spriteGroups["Idle"][0];

        go.AddComponent<Animator>().runtimeAnimatorController = controller;
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
        if (sr.sprite != null)
        {
            col.size = sr.sprite.bounds.size;
            col.offset = sr.sprite.bounds.center;
        }

        Type scriptType = FindType($"{enemyName}Enemy");
        if (scriptType != null) go.AddComponent(scriptType);

        PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction);
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

            // BƯỚC 1: Cấu hình chuẩn Pixel Art và nới lỏng giới hạn kích thước
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            // 🔥 MỞ KHÓA GIỚI HẠN CHO QUÁI TO (TROLL, PANDA, TURTLE...)
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;

            importer.SaveAndReimport();

            // BƯỚC 2: Lấy kích thước ảnh THỰC TẾ
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) continue;

            int width = texture.width;
            int height = texture.height;

            // 🛡️ BẢO VỆ: Bỏ qua ảnh tĩnh hoặc ảnh bị lỗi tỷ lệ
            if (width % height != 0 || width <= height)
            {
                Debug.LogWarning($"[AutoSlice] Bỏ qua ảnh: {texture.name} (Kích thước {width}x{height} không phải dải Animation).");
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                continue;
            }

            // BƯỚC 3: Cắt lưới vuông
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            int cellSize = height;
            int frameCount = width / height;
            List<SpriteRect> rects = new List<SpriteRect>();
            string fileName = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < frameCount; i++)
            {
                rects.Add(new SpriteRect
                {
                    name = $"{fileName}_{i}",
                    rect = new Rect(i * cellSize, 0, cellSize, cellSize),
                    // Đổi tâm về dưới chân để nhân vật không bị nảy lên khi đánh
                    alignment = SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f)
                });
            }

            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
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

    private AnimationClip CreateAnimationClip(List<Sprite> sprites, string actionName, string enemyName, string folderPath)
    {
        AnimationClip clip = new AnimationClip { frameRate = 12 };
        EditorCurveBinding spriteBinding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++) keyframes[i] = new ObjectReferenceKeyframe { time = i / 12f, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        if (actionName == "Attack")
        {
            AnimationEvent ev = new AnimationEvent { functionName = "AnimationEvent_DealDamage", time = (sprites.Count / 2) / 12f };
            AnimationUtility.SetAnimationEvents(clip, new[] { ev });
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = (actionName != "Die" && actionName != "Attack");
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // --- BƯỚC MỚI: CLEAN BUILD ---
        string clipPath = $"{folderPath}/{enemyName}_{actionName}.anim";
        if (AssetDatabase.LoadAssetAtPath(clipPath, typeof(AnimationClip)) != null)
        {
            AssetDatabase.DeleteAsset(clipPath);
        }

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private AnimatorController CreateAnimatorController(Dictionary<string, AnimationClip> clips, string enemyName, string folderPath)
    {
        // --- BƯỚC MỚI: CLEAN BUILD ---
        string controllerPath = $"{folderPath}/{enemyName}_Controller.controller";
        if (AssetDatabase.LoadAssetAtPath(controllerPath, typeof(AnimatorController)) != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;
        Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();

        foreach (var action in actionKeywords)
        {
            if (clips.ContainsKey(action))
            {
                var state = sm.AddState(action);
                state.motion = clips[action];
                states[action] = state;
            }
        }

        if (states.ContainsKey("Idle")) sm.defaultState = states["Idle"];
        if (states.ContainsKey("Attack"))
        {
            sm.AddAnyStateTransition(states["Attack"]).AddCondition(AnimatorConditionMode.If, 0, "Attack");
            states["Attack"].AddTransition(states.ContainsKey("Idle") ? states["Idle"] : states.Values.First()).hasExitTime = true;
        }
        if (states.ContainsKey("Die"))
            sm.AddAnyStateTransition(states["Die"]).AddCondition(AnimatorConditionMode.If, 0, "Die");

        return controller;
    }

    private Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type type = assembly.GetType(typeName) ?? assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                if (type != null) return type;
            }
            catch { }
        }
        return null;
    }
}