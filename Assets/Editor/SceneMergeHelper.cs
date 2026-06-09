using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMergeHelper
{
    [MenuItem("Tools/EsLand/1. Auto-Merge Missing Objects from Main")]
    public static void MergeMissingObjects()
    {
        string currentScenePath = "Assets/Scenes/Map2Kt.unity";
        string mainScenePath = "Assets/Scenes/Map2Kt_Main.unity";

        // Mở scene hiện tại
        Scene currentScene = EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        
        // Mở thêm scene main (binary)
        Scene mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Additive);

        if (!mainScene.IsValid() || !mainScene.isLoaded)
        {
            Debug.LogError("Không thể load scene Map2Kt_Main.unity");
            return;
        }

        GameObject[] currentRoots = currentScene.GetRootGameObjects();
        GameObject[] mainRoots = mainScene.GetRootGameObjects();

        int mergeCount = 0;

        foreach (GameObject mainObj in mainRoots)
        {
            bool exists = false;
            foreach (GameObject currentObj in currentRoots)
            {
                if (mainObj.name == currentObj.name)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                Debug.Log($"[SceneMergeHelper] Phát hiện Object mới từ nhánh main: {mainObj.name}. Đang tiến hành gộp...");
                SceneManager.MoveGameObjectToScene(mainObj, currentScene);
                mergeCount++;
            }
        }

        if (mergeCount > 0)
        {
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[SceneMergeHelper] Đã gộp thành công {mergeCount} object(s) vào {currentScene.name}.");
        }
        else
        {
            Debug.Log("[SceneMergeHelper] Không tìm thấy Object nào khác biệt ở Root để gộp.");
        }

        // Đóng scene main
        EditorSceneManager.CloseScene(mainScene, true);
    }
}
