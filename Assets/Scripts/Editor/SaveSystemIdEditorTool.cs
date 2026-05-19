#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Infrastructure.SaveSystem.Core;

namespace Infrastructure.SaveSystem.EditorTools
{
    [InitializeOnLoad] // 🔥 THÊM DÒNG NÀY: Ép Unity kích hoạt class này chạy ngầm ngay khi load dự án
    public class SaveSystemIdEditorTool : UnityEditor.Editor 
    {
        // 🔥 ĐOẠN CODE ĐĂNG KÝ LUỒNG TỰ ĐỘNG CHẠY KHI BẤM PLAY
        static SaveSystemIdEditorTool()
        {
            // Tháo ra gắn lại để đảm bảo không bị trùng lặp sự kiện trong bộ nhớ Editor
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // Hàm này tự kích hoạt mỗi khi Tiến bấm Play hoặc Stop game
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Ngay lúc ông vừa bấm nút PLAY (Chuẩn bị thoát khỏi Edit Mode để vào Game)
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ValidateAndFixSaveIds(true); // Chạy quét tự động
            }
        }

        // Tạo cái nút bấm thủ công trên thanh công cụ: PBL3 > Quét và Sửa trùng ID Hệ Thống Save
        [MenuItem("PBL3/Quét và Sửa trùng ID Hệ Thống Save")]
        public static void ManualValidate()
        {
            ValidateAndFixSaveIds(false); // Chạy quét thủ công
        }

        // Hàm lõi xử lý quét và sửa ID
        public static void ValidateAndFixSaveIds(bool isAuto)
        {
            SaveableEntity[] allEntities = Object.FindObjectsOfType<SaveableEntity>(true);
            HashSet<string> checkedIds = new HashSet<string>();
            int fixCount = 0;

            foreach (var entity in allEntities)
            {
                // Nếu trống hoặc bị trùng với ID đã quét qua trước đó
                if (string.IsNullOrEmpty(entity.Id) || checkedIds.Contains(entity.Id))
                {
                    entity.GenerateGuid(); // Ép sinh mã mới tinh
                    fixCount++;
                }
                
                checkedIds.Add(entity.Id);
            }

            // XỬ LÝ LOG THÔNG MINH ĐỂ ĐỠ RÁC CONSOLE:
            // 1. Nếu có lỗi: Dù tự động hay thủ công cũng sẽ in chữ màu xanh cyan báo cáo cho ông thấy
            if (fixCount > 0)
            {
                string tag = isAuto ? "TỰ ĐỘNG" : "THỦ CÔNG";
                Debug.Log($"<color=cyan>[SaveSystem Tool] {tag} QUÉT: Đã sửa thành công {fixCount} object bị trùng/trống ID.</color>");
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
            // 2. Nếu map đã sạch sẽ: Chỉ in log khi ông chủ động bấm tay (để biết tool có chạy), chạy tự động thì im lặng cho sạch sẽ
            else if (!isAuto)
            {
                Debug.Log("<color=green>[SaveSystem Tool] THỦ CÔNG QUÉT: Tuyệt vời! Không phát hiện ID nào bị trùng hay trống.</color>");
            }
        }
    }
}
#endif