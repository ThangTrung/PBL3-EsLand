# Plan: Final Development Phase Execution

## Phase 1: Endgame Loop (Thoát Đảo)
- **Step 1.1: Fix Enemy Loot Spawning.**
  - Chỉnh sửa `Enemy.cs`: Thay vì in Log, hãy Instantiate một `ItemPickup` prefab khi rớt đá.
- **Step 1.2: Create EscapeBoatController.**
  - Gắn vào `Boat.prefab`. Khi `Interact`, kiểm tra nếu đây là Thuyền đã chế tạo xong -> Hiện UI Thắng cuộc.
- **Step 1.3: Update GameManager.**
  - Thêm trạng thái `IsGameCompleted`. 
  - Hàm `HandleVictory()`: Dừng `Time.timeScale = 0`, hiện `VictoryPanel`.

## Phase 2: Atmosphere (Ngày & Đêm)
- **Step 2.1: Implement TimeManager.**
  - Singleton quản lý biến `float currentTime`.
  - Event `OnHourChanged` để các hệ thống khác lắng nghe.
- **Step 2.2: URP Lighting Integration.**
  - Tạo một `LightCycleController` điều khiển `Global Light 2D` bằng `Gradient` (Vàng -> Cam -> Tím -> Xanh).
- **Step 2.3: Sleep Logic Refinement.**
  - Cập nhật `HomeSavePoint.cs`: Khi ngủ, gọi `TimeManager.Instance.SkipToMorning()`.

## Phase 3: Game Feel (Polish)
- **Step 3.1: Camera Shake Script.**
  - Viết script dùng `transform.localPosition` cộng với `Random.insideUnitSphere`.
  - Đăng ký sự kiện rung khi Player nhận sát thương.
- **Step 3.2: Hit Particles.**
  - Tạo `EffectManager` đơn giản để quản lý việc spawn Particle từ Object Pool.
  - Tích hợp vào `CharacterHealth.TakeDamage`.

## Phase 4: AI & Deployment
- **Step 4.1: TorchGoblin Configuration.**
  - Mở prefab `TorchGoblin`, gắn `Enemy` script.
  - Cấu hình `AttackStrategy` là `RangedProjectileAttackStrategy`.
- **Step 4.2: Final Integration Test.**
  - Test luồng: Đánh Boss -> Rớt Đá -> Chế Thuyền -> Thắng.

---

## Tasks
- [ ] **Task 1: Hệ thống Thoát đảo & Victory UI.**
- [ ] **Task 2: Hệ thống Thời gian & Ánh sáng Ngày/Đêm.**
- [ ] **Task 3: Camera Shake & Combat VFX.**
- [ ] **Task 4: Cấu hình quái đánh xa (TorchGoblin).**
