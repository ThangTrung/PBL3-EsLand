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
                    // TODO: Gọi hàm lưu game sau này
                    break;
                case SettingsActionType.Exit:
                    // TODO: Gọi hàm thoát game sau này
                    break;
                case SettingsActionType.Custom:
                    // TODO: Xử lý các nút tùy chỉnh sau này (ví dụ: Đăng xuất)
                    break;
            }
        }
    }
}
