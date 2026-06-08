# Plan: Thực hiện Contextual Settings Menu

## 1. Thành phần Dữ liệu (Data)
- Tạo `SettingsActionType` (enum) cho các hành động phổ biến: `Save`, `Load`, `Exit`, `Settings`, `Help`.
- Tạo `SettingsActionData` (class) chứa thông tin hiển thị của một nút.
- Tạo `SettingsActionConfig` (ScriptableObject) chứa danh sách `SettingsActionData` theo từng Scene.

## 2. Thành phần Hiển thị (UI)
- Tạo `SettingsMenuUI`:
    - Quản lý 4 nút con (đã có sẵn trong Prefab nhưng ẩn đi).
    - Hàm `Setup(List<SettingsActionData> actions)`: Cập nhật Text/Icon và Bật/Tắt nút dựa trên số lượng action.
    - Xử lý đóng menu khi bấm ra ngoài (Blocker).
- Tạo `SettingsButtonController`:
    - Gắn vào nút chính.
    - Tìm kiếm Config phù hợp với Scene hiện tại.
    - Khi bấm, truyền dữ liệu sang `SettingsMenuUI`.

## 3. Thành phần Xử lý (Logic)
- Tạo `SettingsActionHandler`:
    - Chứa các hàm thực thi cho từng `SettingsActionType`.
    - Ví dụ: `HandleSave()` gọi `SaveLoadManager.Instance.SaveGame()`.

## 4. Thứ tự thực hiện
1. Viết các lớp Data (ScriptableObject).
2. Viết `SettingsMenuUI` (View).
3. Viết `SettingsActionHandler` (Logic).
4. Viết `SettingsButtonController` (Controller).
5. Hướng dẫn thiết lập Prefab trong Unity Editor.
