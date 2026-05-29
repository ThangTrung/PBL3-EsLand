using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PBL3.EditorTools
{
    public class PrefabComponentSynchronizer : EditorWindow
    {
        [Header("Settings")]
        public GameObject templatePrefab;
        public List<GameObject> targetPrefabs = new List<GameObject>();

        // Quản lý GUI Inspector
        private SerializedObject _serializedObject;
        private SerializedProperty _templateProp;
        private SerializedProperty _targetsProp;
        private Vector2 _scrollPos;

        [MenuItem("Tools/PBL3/Sync Bronze Prefabs")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabComponentSynchronizer>("Sync Prefabs");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // Sử dụng SerializedObject để vẽ UI List đẹp và chuẩn xác như Inspector mặc định
            _serializedObject = new SerializedObject(this);
            _templateProp = _serializedObject.FindProperty("templatePrefab");
            _targetsProp = _serializedObject.FindProperty("targetPrefabs");
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("1. Reference Setup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_templateProp, new GUIContent("Template Prefab (Gold)"));

            EditorGUILayout.Space(5);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            EditorGUILayout.PropertyField(_targetsProp, new GUIContent("Target Prefabs (Bronze)"), true);
            EditorGUILayout.EndScrollView();

            _serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("2. Action", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); // Nút màu xanh cho nổi bật
            if (GUILayout.Button("Sync Components", GUILayout.Height(40)))
            {
                ExecuteSync();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ExecuteSync()
        {
            if (templatePrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Vui lòng kéo Template Prefab vào ô trống!", "OK");
                return;
            }

            if (targetPrefabs == null || targetPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Vui lòng kéo ít nhất 1 Target Prefab vào danh sách!", "OK");
                return;
            }

            int syncedCount = 0;
            Component[] sourceComponents = templatePrefab.GetComponents<Component>();

            // Gom tất cả thao tác vào 1 group Undo duy nhất để Ctrl+Z một phát là hoàn tác toàn bộ
            Undo.SetCurrentGroupName("Sync Prefab Components");
            int undoGroup = Undo.GetCurrentGroup();

            try
            {
                for (int i = 0; i < targetPrefabs.Count; i++)
                {
                    GameObject target = targetPrefabs[i];
                    if (target == null) continue;

                    // Hiển thị thanh tiến trình
                    EditorUtility.DisplayProgressBar("Syncing Prefabs", $"Processing {target.name}...", (float)i / targetPrefabs.Count);

                    // Bắt buộc phải là Prefab Asset trong Project, không chơi với object trên Scene
                    if (!PrefabUtility.IsPartOfPrefabAsset(target))
                    {
                        continue;
                    }

                    Undo.RegisterCompleteObjectUndo(target, "Record Original Target");

                    foreach (Component sourceComp in sourceComponents)
                    {
                        // Tuyệt đối bỏ qua Transform, chúng ta không muốn copy vị trí/tỷ lệ của cục Gold sang cục Bronze
                        if (sourceComp is Transform) continue;

                        System.Type compType = sourceComp.GetType();
                        Component targetComp = target.GetComponent(compType);

                        bool isNewComponent = false;

                        // 1. Thêm Component nếu chưa có
                        if (targetComp == null)
                        {
                            targetComp = Undo.AddComponent(target, compType);
                            isNewComponent = true;
                        }

                        // 2. Copy toàn bộ giá trị (Kể cả private serialize field) từ Template sang Target
                        Undo.RecordObject(targetComp, "Sync Component Values");
                        EditorUtility.CopySerializedIfDifferent(sourceComp, targetComp);

                        if (isNewComponent)
                        {
                        }
                    }

                    // Đánh dấu là Prefab này đã bị thay đổi (Dirty) để Unity biết đường Save
                    EditorUtility.SetDirty(target);
                    // Force Save Prefab
                    PrefabUtility.SavePrefabAsset(target);

                    syncedCount++;
                }
            }
            finally
            {
                // Dọn dẹp thanh tiến trình dù code có chạy lỗi hay thành công
                EditorUtility.ClearProgressBar();
                Undo.CollapseUndoOperations(undoGroup);

                // Refresh lại AssetDatabase để hiện thay đổi ra Editor ngay lập tức
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Success", $"Đã đồng bộ thành công cho {syncedCount} Prefabs!\n\nNếu có lỗi, bạn có thể bấm Ctrl + Z để hoàn tác.", "Tuyệt vời!");
        }
    }
}
