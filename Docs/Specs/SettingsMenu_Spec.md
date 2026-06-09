# Spec: Hệ thống Menu Cấu hình Đa ngữ cảnh (Contextual Settings Menu)

## Objective
Tạo một hệ thống nút Setting thông minh, có khả năng thay đổi các tùy chọn sổ ra tùy thuộc vào Scene hiện tại. Hệ thống phải đảm bảo tính mở rộng cao (thêm Scene mới không cần sửa code cũ) và tách biệt rõ ràng giữa hiển thị (UI) và xử lý logic (Action).

## Tech Stack
- Unity 2022.3.62f3
- C# (SOLID principles)
- ScriptableObjects (Data-driven)

## Commands
- N/A (Unity Editor workflow)

## Project Structure
- `Assets/Scripts/UI/Settings/`
    - `SettingsActionConfig.cs` (ScriptableObject)
    - `SettingsMenuUI.cs` (View - Hiển thị các nút con)
    - `SettingsButtonController.cs` (Controller - Điều phối nút chính)
    - `SettingsActionHandler.cs` (Logic - Xử lý khi bấm nút)
- `Assets/Prefabs/UI/Settings/`
    - `SettingsButton.prefab`
    - `SettingsSubMenu.prefab`

## Code Style
```csharp
namespace UI.Settings
{
    public class SettingsMenuUI : MonoBehaviour
    {
        [SerializeField] private List<Button> subButtons;
        // logic hiển thị...
    }
}
```

## Testing Strategy
- Kiểm tra hiển thị nút ở Scene "Map1" (đã có cấu hình).
- Kiểm tra hiển thị nút ở Scene mới chưa có cấu hình (trường hợp fallback).
- Kiểm tra logic khi click từng nút Action.

## Boundaries
- **Always do:** Sử dụng `SceneManager.GetActiveScene().name` để nhận diện ngữ cảnh.
- **Ask first:** Nếu muốn thêm các Action yêu cầu truyền dữ liệu phức tạp.
- **Never do:** Gắn trực tiếp logic vào hàm `onClick` của Button trong Inspector.

## Success Criteria
- Bấm nút Setting chính sẽ sổ ra Menu con.
- Số lượng nút con thay đổi theo Scene (1-4 nút).
- Menu con tự đóng khi click ra ngoài hoặc chọn một Action.
- Dễ dàng thêm Action mới chỉ bằng cách tạo ScriptableObject.
