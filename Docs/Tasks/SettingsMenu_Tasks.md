# Tasks: Triển khai Contextual Settings Menu

- [ ] **Task 1: Xây dựng cấu trúc dữ liệu (Data)**
    - Tạo `SettingsActionConfig.cs` và `SettingsActionType`.
    - Acceptance: Có thể tạo file ScriptableObject trong Unity Editor và thêm tối đa 4 action.
    - Verify: Click chuột phải -> Create -> UI -> Settings Config.

- [ ] **Task 2: Lập trình View (SettingsMenuUI)**
    - Tạo `SettingsMenuUI.cs` quản lý các nút con và Blocker.
    - Acceptance: Menu con có thể hiện/ẩn và cập nhật nhãn nút động.
    - Files: `Assets/Scripts/UI/Settings/SettingsMenuUI.cs`

- [ ] **Task 3: Lập trình Logic (SettingsActionHandler)**
    - Tạo `SettingsActionHandler.cs` để thực thi các lệnh.
    - Acceptance: Bấm nút "Save" thì gọi được SaveGame, bấm "Exit" thì thoát được ứng dụng.
    - Files: `Assets/Scripts/UI/Settings/SettingsActionHandler.cs`

- [ ] **Task 4: Lập trình Controller (SettingsButtonController)**
    - Tạo `SettingsButtonController.cs` nhận diện Scene.
    - Acceptance: Nút chính nhận diện đúng Scene và truyền đúng Config vào Menu UI.
    - Files: `Assets/Scripts/UI/Settings/SettingsButtonController.cs`

- [ ] **Task 5: Thiết lập Prefab & UI (Manual)**
    - Hướng dẫn tạo Prefab Nút chính và Bảng menu con.
    - Verify: Chạy game ở các Scene khác nhau và kiểm tra số lượng nút con.
