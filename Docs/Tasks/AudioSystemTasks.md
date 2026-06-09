# Task List: Triển khai Hệ thống Âm thanh (Audio System)

## Phase 1: Nền tảng & Dữ liệu (Foundations & Data)
- [ ] **Task 1.1: Định nghĩa AudioData**
  - Mô tả: Tạo ScriptableObject để cấu hình âm thanh.
  - File: `Assets/Scripts/Data/Audio/AudioData.cs`
  - Verify: Có thể tạo mới AudioData từ menu Create trong Unity.

- [ ] **Task 1.2: Tạo Interface IAudioService**
  - Mô tả: Định nghĩa các hàm giao tiếp cho hệ thống Audio.
  - File: `Assets/Scripts/Core/Contracts/IAudioService.cs`
  - Verify: Script biên dịch thành công.

- [ ] **Task 1.3: Khởi tạo cấu trúc thư mục Assets**
  - Mô tả: Tạo các thư mục Audio/SFX, Audio/BGM, Data/Audio.
  - Verify: Thư mục xuất hiện trong Project window.

## Phase 2: Triển khai Hạ tầng (Infrastructure)
- [ ] **Task 2.1: Thiết lập AudioMixer**
  - Mô tả: Tạo AudioMixer với các nhóm Master, SFX, BGM.
  - Verify: AudioMixer tồn tại trong Assets/Settings.

- [ ] **Task 2.2: Lập trình AudioManager & Pool**
  - Mô tả: Viết logic phát âm thanh và quản lý Pooling.
  - File: `Assets/Scripts/Infrastructure/Audio/AudioManager.cs`
  - Verify: Lấy được AudioSource từ Pool khi gọi PlaySFX.

## Phase 3: Kết nối Gameplay (Integration)
- [ ] **Task 3.1: Tạo Cầu nối Âm thanh (Audio Listener)**
  - Mô tả: Tạo script lắng nghe sự kiện gameplay để phát âm thanh.
  - File: `Assets/Scripts/Gameplay/Audio/BaseAudioListener.cs`
  - Verify: Gắn được vào Prefab nhân vật.

- [ ] **Task 3.2: Thử nghiệm thực tế**
  - Mô tả: Gắn âm thanh vào hành động Chém hoặc Nhặt đồ.
  - Verify: Âm thanh phát ra trong PlayMode khi thực hiện hành động.

## Phase 4: Hoàn thiện (Polishing)
- [ ] **Task 4.1: UI Settings**
  - Mô tả: Tạo Slider điều chỉnh âm lượng.
  - Verify: Kéo Slider làm thay đổi âm lượng thực tế.
