# Spec: Project Finalization (Endgame, Atmosphere, & Game Feel)

## 1. Objective
Hoàn thiện dự án PBL3 bằng cách kết nối các mảnh ghép logic rời rạc thành một vòng lặp game hoàn chỉnh: Người chơi sinh tồn qua Ngày/Đêm -> Chiến đấu với Boss để thu thập Đá Sức Mạnh -> Chế tạo Thuyền -> Thoát khỏi đảo (Chiến thắng). Đồng thời nâng cấp trải nghiệm người dùng thông qua hiệu ứng chiến đấu (Game Feel).

---

## 2. Hệ thống Thoát đảo (Endgame Condition)
### 2.1. Boss Loot
- **Cơ chế:** Khi 4 Boss chính bị tiêu diệt, mỗi con rớt ra 1 loại Đá Sức Mạnh duy nhất (tỉ lệ 100%).
- **Lớp thực thi:** `Enemy.cs` (Cập nhật logic `DropLoot`).
- **Dữ liệu:** Sử dụng `ItemData` có sẵn cho các Power Stones.

### 2.2. Victory Trigger
- **Cơ chế:** Sau khi dùng 4 viên đá chế tạo Thuyền, người chơi click vào Thuyền để thoát đảo.
- **Lớp thực thi:** `EscapeBoatController.cs` (mới) triển khai `IInteractable`.
- **Hành động:** Gọi `GameManager.Instance.CompleteGame()`.

---

## 3. Chu kỳ Ngày/Đêm & Hệ thống Nghủ
### 3.1. Time Manager
- **Cơ chế:** Quản lý thời gian thực trong game (vòng lặp 24h). 1 phút thực tế = 1 giờ trong game (tùy chỉnh).
- **Lớp thực thi:** `TimeManager.cs` (mới).
- **Lighting:** Điều khiển `Intensity` và `Color` của **Global Light 2D** (URP).
    - *Sáng:* Vàng nhạt, Cường độ 1.0.
    - *Đêm:* Xanh đậm, Cường độ 0.2.

### 3.2. Sleeping System
- **Cơ chế:** Tương tác với Nhà (`HomeSavePoint`) để ngủ.
- **Hành động:** 
    - Chuyển thời gian đến 6:00 sáng hôm sau.
    - Hồi đầy Máu và Thể lực.
    - Thực hiện lưu game (Save Game).

---

## 4. Nâng cấp Phản hồi Chiến đấu (Combat Polish)
### 4.1. Camera Shake
- **Cơ chế:** Rung màn hình khi người chơi gây sát thương hoặc nhận sát thương.
- **Lớp thực thi:** `CameraShake.cs` (mới) gắn vào Main Camera.
- **Kích hoạt:** Gọi từ `CharacterHealth.OnDamaged`.

### 4.2. Hit VFX & SFX
- **VFX:** Sinh ra Particle FX tại vị trí va chạm (Sử dụng `Assets/Textures/Tiny Swords/Particle FX/`).
- **SFX:** Phát âm thanh chém/đập (Sử dụng `AudioManager` nếu có, hoặc `AudioSource` tạm thời).

---

## 5. Success Criteria
1. Người chơi có thể thắng game và thấy màn hình Victory sau khi chế thuyền.
2. Ánh sáng môi trường thay đổi mượt mà theo thời gian.
3. Cảm giác đánh quái "đã" hơn nhờ có hiệu ứng rung màn hình và tóe bụi/máu.
4. Ngủ qua đêm hoạt động đúng logic (hồi chỉ số + nhảy giờ).
