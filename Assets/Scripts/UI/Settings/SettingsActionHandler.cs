using UnityEngine;

namespace UI.Settings
{
    public class SettingsActionHandler : MonoBehaviour
    {
        public void ExecuteAction(SettingsActionData action)
        {
            if (action == null) return;

            // Hiện tại chỉ in ra Console để kiểm tra logic nhận diện nút.
            // Sau này bạn sẽ viết logic thật (Lưu, Thoát...) vào đây.
            Debug.Log($"[Settings] Người chơi vừa bấm nút: {action.actionType} (Nhãn: {action.label})");

            switch (action.actionType)
            {
                case SettingsActionType.Save:
                    if (Infrastructure.SaveSystem.Core.SaveLoadManager.Instance != null)
                    {
                        Infrastructure.SaveSystem.Core.SaveLoadManager.Instance.SyncToCloudManual((success, msg) => {
                            if (success) Debug.Log($"[Settings] Đã lưu game thành công: {msg}");
                            else Debug.LogWarning($"[Settings] Lỗi khi lưu game: {msg}");
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[Settings] Không tìm thấy SaveLoadManager để lưu game.");
                    }
                    break;
                case SettingsActionType.Exit:
                    Debug.Log("[Settings] Đang thoát game...");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                case SettingsActionType.LogOut:
                    if (Infrastructure.SaveSystem.Core.SaveLoadManager.Instance != null)
                    {
                        // 1. Lưu game lên cloud lần cuối
                        Infrastructure.SaveSystem.Core.SaveLoadManager.Instance.SyncToCloudManual((success, msg) => {
                            Debug.Log($"[Settings] Đã lưu tiến trình trước khi đăng xuất: {msg}");
                            
                            // 2. Xóa sạch dữ liệu trong bộ nhớ (Reset System)
                            Infrastructure.SaveSystem.Core.SaveLoadManager.Instance.ResetSystem();
                            
                            // 3. Xóa thông tin đăng nhập
                            Infrastructure.SaveSystem.Core.CloudAuthService.Logout();
                            
                            // 4. Chuyển về màn hình Login
                            Core.SceneManagement.SceneLoader.Load("Login");
                        });
                    }
                    else
                    {
                        Infrastructure.SaveSystem.Core.CloudAuthService.Logout();
                        Core.SceneManagement.SceneLoader.Load("Login");
                    }
                    break;
                case SettingsActionType.Custom:
                    // TODO: Xử lý các nút tùy chỉnh sau này (ví dụ: Đăng xuất)
                    break;
            }
        }
    }
}
