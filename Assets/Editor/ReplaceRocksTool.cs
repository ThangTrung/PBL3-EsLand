using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ReplaceRocksTool : EditorWindow
{
    private const string MAP_SCENE_NAME = "Map2Kt";
    
    // GUIDs of decoration rocks to be replaced
    private static readonly string[] DECORATION_ROCK_GUIDS = {
        "e918cdd58b67733468ffc30e6c71755a", // Rock_2
        "906030faf14477640a2294d88e9408c6", // Rock_3
        "6323a908e3d84784db1599c4b18e27cb"  // Rock_4
    };

    // Paths to resource prefabs
    private static readonly string[] RESOURCE_ROCK_PATHS = {
        "Assets/Prefabs/Resource/Rock/rock_2.prefab",
        "Assets/Prefabs/Resource/Rock/rock_3.prefab",
        "Assets/Prefabs/Resource/Rock/rock_4.prefab",
        "Assets/Prefabs/Resource/Rock/rock_5.prefab",
        "Assets/Prefabs/Resource/Rock/rock_6.prefab"
    };

    private static readonly string[] RESOURCE_IRON_PATHS = {
        "Assets/Prefabs/Resource/Iron/Iron_Resource_1.prefab",
        "Assets/Prefabs/Resource/Iron/Iron_Resource_2.prefab",
        "Assets/Prefabs/Resource/Iron/Iron_Resource_3.prefab",
        "Assets/Prefabs/Resource/Iron/Iron_Resource_4.prefab",
        "Assets/Prefabs/Resource/Iron/Iron_Resource_5.prefab",
        "Assets/Prefabs/Resource/Iron/Iron_Resource_6.prefab"
    };

    [MenuItem("Tools/PBL3/Replace Decoration Rocks in Map2Kt")]
    public static void ReplaceRocks()
    {
        string currentScene = EditorSceneManager.GetActiveScene().name;
        if (currentScene != MAP_SCENE_NAME)
        {
            if (!EditorUtility.DisplayDialog("Cảnh báo Scene", 
                $"Bạn đang ở scene '{currentScene}'. Công cụ này được thiết kế cho '{MAP_SCENE_NAME}'. Bạn có muốn tiếp tục không?", "Tiếp tục", "Hủy"))
            {
                return;
            }
        }

        // Load replacement prefabs
        GameObject[] rockPrefabs = RESOURCE_ROCK_PATHS.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(p => p != null).ToArray();
        GameObject[] ironPrefabs = RESOURCE_IRON_PATHS.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(p => p != null).ToArray();

        if (rockPrefabs.Length == 0 || ironPrefabs.Length == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy các Prefab tài nguyên trong thư mục Resource. Vui lòng kiểm tra lại đường dẫn.", "OK");
            return;
        }

        // Find all objects in scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> objectsToReplace = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
            {
                GameObject prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefabParent != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefabParent));
                    if (DECORATION_ROCK_GUIDS.Contains(guid))
                    {
                        objectsToReplace.Add(obj);
                    }
                }
            }
        }

        int total = objectsToReplace.Count;
        if (total == 0)
        {
            EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy đối tượng Rock trang trí nào cần thay thế.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Xác nhận thay thế", 
            $"Tìm thấy {total} đối tượng Rock trang trí. Bạn có chắc chắn muốn thay thế chúng bằng Rock/Iron tài nguyên (tỉ lệ 6:4) không?", "Thực hiện", "Hủy"))
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Replace Decoration Rocks");
        int groupIndex = Undo.GetCurrentGroup();

        try
        {
            for (int i = 0; i < total; i++)
            {
                GameObject oldObj = objectsToReplace[i];
                if (oldObj == null) continue;

                EditorUtility.DisplayProgressBar("Đang thay thế Rock", $"Tiến độ: {i + 1}/{total}", (float)i / total);

                // Decide type based on 6:4 ratio
                GameObject selectedPrefab;
                if (Random.value <= 0.6f) // 60% Rock
                {
                    selectedPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                }
                else // 40% Iron
                {
                    selectedPrefab = ironPrefabs[Random.Range(0, ironPrefabs.Length)];
                }

                // Instantiate new prefab
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, oldObj.transform.parent);
                newObj.transform.position = oldObj.transform.position;
                newObj.transform.rotation = oldObj.transform.rotation;
                newObj.transform.localScale = oldObj.transform.localScale;
                newObj.name = selectedPrefab.name;

                Undo.RegisterCreatedObjectUndo(newObj, "Create Resource");
                Undo.DestroyObjectImmediate(oldObj);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Thành công", $"Đã thay thế thành công {total} đối tượng.", "Tuyệt vời");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            Undo.CollapseUndoOperations(groupIndex);
        }
    }
}
