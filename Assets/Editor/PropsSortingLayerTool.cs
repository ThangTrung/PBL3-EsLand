using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PropsSortingLayerTool
{
    private struct SortingTask
    {
        public string rootName;
        public string layerName;

        public SortingTask(string root, string layer)
        {
            rootName = root;
            layerName = layer;
        }
    }

    private static readonly SortingTask[] tasks = new SortingTask[]
    {
        new SortingTask("Props_A", "Elevation_A"),
        new SortingTask("Props_B", "Elevation_B"),
        new SortingTask("Props_C", "Elevation_C")
    };

    [MenuItem("Tools/PBL3/Sorting Layers/Set All Props Sorting Layers")]
    public static void SetAllSortingLayers()
    {
        int totalUpdated = 0;
        foreach (var task in tasks)
        {
            totalUpdated += ExecuteTask(task, false);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Success", $"Finished updating sorting layers.\nTotal renderers updated: {totalUpdated}", "OK");
    }

    [MenuItem("Tools/PBL3/Sorting Layers/Set Props_A -> Elevation_A")]
    public static void SetPropsA() => ExecuteTask(tasks[0], true);

    [MenuItem("Tools/PBL3/Sorting Layers/Set Props_B -> Elevation_B")]
    public static void SetPropsB() => ExecuteTask(tasks[1], true);

    [MenuItem("Tools/PBL3/Sorting Layers/Set Props_C -> Elevation_C")]
    public static void SetPropsC() => ExecuteTask(tasks[2], true);

    private static int ExecuteTask(SortingTask task, bool showDialog)
    {
        GameObject root = GameObject.Find(task.rootName);
        
        if (root == null)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Error", $"Could not find GameObject named '{task.rootName}' in the current scene.", "OK");
            return 0;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        int count = 0;

        foreach (var renderer in renderers)
        {
            if (renderer.sortingLayerName != task.layerName)
            {
                Undo.RecordObject(renderer, $"Change Sorting Layer of {task.rootName}");
                renderer.sortingLayerName = task.layerName;
                count++;
            }
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            if (showDialog)
                EditorUtility.DisplayDialog("Success", $"Successfully updated Sorting Layer to '{task.layerName}' for {count} renderers under '{task.rootName}'.", "OK");
        }
        else if (showDialog)
        {
            EditorUtility.DisplayDialog("Info", $"All renderers under '{task.rootName}' are already on '{task.layerName}'.", "OK");
        }

        return count;
    }
}
