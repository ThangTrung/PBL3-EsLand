# Plan: Implementing Centralized Survival Settings

## Overview
Chuyển đổi các thông số sinh tồn từ biến cục bộ sang hệ thống ScriptableObject tập trung.

## Implementation Order

### 1. Data Structure
- Tạo thư mục `Assets/Scripts/Data/Survival/`.
- Viết script `SurvivalSettings.cs` với đầy đủ các trường dữ liệu cần thiết.
- Tạo một file Asset mẫu tại `Assets/Settings/Survival/DefaultSurvivalSettings.asset`.

### 2. Refactoring Controllers
- **Update PlayerSurvivalController:** Thay thế các `[SerializeField]` hiện có bằng một tham chiếu duy nhất tới `SurvivalSettings`. Cập nhật code để đọc dữ liệu từ đó.
- **Update PlayerInteractionController:** Xóa biến `thirstRestoreAmount` cục bộ và đọc nó từ `SurvivalSettings` của người chơi.

### 3. Verification
- Chạy game và kiểm tra xem các chỉ số vẫn hoạt động (tiêu hao, hồi phục).
- Thử thay đổi giá trị trong file Asset lúc đang Play (nếu được) hoặc giữa các lần chạy để xác nhận tính năng "tùy chỉnh dễ dàng".

## Tasks
- [ ] Task 1: Tạo cấu trúc `SurvivalSettings` và file Asset.
- [ ] Task 2: Cập nhật `PlayerSurvivalController.cs`.
- [ ] Task 3: Cập nhật `PlayerInteractionController.cs`.
