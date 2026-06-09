# Spec: Luồng Đăng nhập & Chuyển cảnh (Login Flow)

## Objective
Xây dựng luồng khởi chạy game đồng bộ với Backend (Node.js/SQL Server) theo thứ tự: **Login -> Loading -> MainMenu -> Loading -> Map2Kt**.
- Tại màn hình **Login**: Người dùng nhập Tài khoản (Username) và Mật khẩu (Password). Game gửi request tới Backend (IP máy bạn).
  - Backend tự động xử lý: Nếu tồn tại thì đăng nhập, nếu chưa thì tạo tài khoản mới.
- Tại màn hình **Loading**: Chuyển tiếp mượt mà và load dữ liệu người chơi (Cloud Save).
- Tại màn hình **MainMenu**: 
  - Nếu tài khoản đã có dữ liệu lưu (trình độ chơi cũ): Nút "Tiếp tục" (Continue) sáng lên và cho phép chơi tiếp.
  - Nếu là tài khoản mới (hoặc chưa có save): Nút "Tiếp tục" bị mờ/khóa, bắt buộc (hoặc ưu tiên) nhấn "Trò chơi mới" (New Game).
- Nhấn một trong hai nút sẽ đi qua **Loading** lần nữa để vào **Map2Kt**.

## Tech Stack
- **Client**: Unity 2D (C#).
- **Backend**: Node.js (Express), SQL Server (đã có sẵn API `/api/auth/login` và `/api/loadgame/:userID`).
- **Scene Management**: Sử dụng `SceneLoader.cs` hiện tại để trung chuyển qua scene `Loading`.

## Commands
- Thực thi game từ Scene: `Login.unity` (Build Index 0).

## Project Structure
Các file chịu ảnh hưởng chính:
- `Assets/Scripts/UI/Login/LoginController.cs`: Cập nhật logic để gửi request login, khởi tạo cấu hình Cloud và gọi `SceneLoader.Load("MainMenu")`. Sáp nhập các tính năng IP config nếu cần từ `LoginUIController.cs`.
- `Assets/Scripts/UI/MainMenu/MainMenuController.cs`: Đọc `SaveLoadManager.Instance.HasData()` để cập nhật state của nút "Tiếp tục".
- `Assets/Scripts/Infrastructure/SaveSystem/Core/SaveLoadManager.cs`: Đảm bảo `HasData()` hoạt động chuẩn xác dựa trên dữ liệu tải từ Cloud về.

## Code Style
```csharp
// Sử dụng chuẩn UnityWebRequest với Coroutine. 
// Tách biệt logic UI và Logic Network:
public void OnLoginClick()
{
    StartCoroutine(CloudAuthService.Login(username, password, (success, message) => 
    {
        if (success)
        {
            // Thiết lập user ID và load cloud data
            SaveLoadManager.Instance.EnableCloudMode(CloudAuthService.CurrentUserID, serverIP);
            SceneLoader.Load("MainMenu");
        }
    }));
}
```

## Testing Strategy
- **Manual Testing**: Chạy thẳng từ Scene Login.
- **Test case 1 (Tài khoản mới)**: Nhập acc mới -> Server trả về `201` -> Chuyển sang Main Menu -> Nút Continue khóa -> Nhấn New Game -> Tới Map2Kt.
- **Test case 2 (Tài khoản cũ)**: Nhập acc cũ -> Server trả về `200` kèm data -> Chuyển sang Main Menu -> Nút Continue mở -> Nhấn Continue -> Tới Map2Kt.

## Boundaries
- **Luôn luôn**: Sử dụng `SceneLoader.Load` thay vì `SceneManager.LoadScene` trực tiếp để đảm bảo luôn đi qua màn hình Loading. Đảm bảo bảo mật tối thiểu hoặc thông báo rõ ràng nếu mất kết nối server.
- **Hỏi trước**: Thay đổi cấu trúc cơ sở dữ liệu trên server, hoặc thay đổi Build Index của Scene.
- **Không bao giờ**: Hardcode Server IP thẳng vào code production mà không có chỗ cấu hình (Nên để field nhập IP cho demo hoặc lưu trong ScriptableObject/PlayerPrefs).

## Success Criteria
- [ ] Mở game lên, scene Login hiện ra đầu tiên.
- [ ] Đăng nhập thành công, Cloud data tự tải ngầm.
- [ ] Chuyển qua scene Loading rồi sang MainMenu.
- [ ] Nút "Tiếp tục" hoạt động chính xác dựa trên trạng thái dữ liệu của tài khoản.
- [ ] Khi chọn "New Game" hoặc "Continue", game qua scene Loading rồi vào Map2Kt thành công với dữ liệu tương ứng.

## Open Questions & Decisions
- **Server IP**: Đã chuyển sang quản lý qua ScriptableObject tĩnh `NetworkSettings.asset` đặt tại `Assets/Resources`. Không hiển thị ô nhập IP trên giao diện Login.
- **Trò chơi mới (New Game)**: Khi người chơi cố tình bấm "Trò chơi mới", game sẽ tự động tạo dữ liệu trắng và lưu đè lên Cloud ngay lập tức, xóa hoàn toàn tiến trình cũ.
- **Giao diện**: Các liên kết UI đã được giữ nguyên trên Prefab/Scene hiện tại. Logic xử lý IP ở giao diện cũ đã được loại bỏ hoàn toàn.