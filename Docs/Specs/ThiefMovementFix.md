# Spec: Sửa lỗi di chuyển của Thief Enemy (Phân tích & Giải quyết Gốc rễ)

## 1. Objective (Mục tiêu)
Khắc phục triệt để lỗi quái vật Thief (và các quái vật khác) không di chuyển hoặc bị khựng lại trong quá trình tuần tra/đuổi theo mục tiêu. Đảm bảo hệ thống AI hoạt động trơn tru, tuân thủ SOLID và kiến trúc hiện tại của dự án.

## 2. Analysis (Phân tích Gốc rễ)
Sau khi kiểm tra mã nguồn, tôi phát hiện 3 vấn đề nghiêm trọng dẫn đến "lỗi lạ" này:

### A. Mismatch Ngưỡng Khoảng Cách (Threshold Conflict)
- `EnemyBase` định nghĩa `DefaultPatrolReachDistance = 0.3f`.
- `EnemyMovementController` mặc định `_stopDistance = 0.5f`.
- **Hậu quả:** Khi tuần tra, `MovementController` sẽ dừng quái lại khi cách đích 0.5m. Tuy nhiên, `PatrolState` vẫn đợi quái đến gần 0.3m mới coi là "đã đến" để chọn điểm mới. Quái bị kẹt ở khoảng cách 0.5m mãi mãi vì `MovementController` đã tắt lệnh di chuyển (`_isNavigating = false`).

### B. Logic Cờ Hiệu (State Flag Desync) trong PatrolState
- `PatrolState` sử dụng cờ `_isMovingToPoint` để chỉ gọi lệnh di chuyển 1 lần.
- **Hậu quả:** Nếu vì bất kỳ lý do gì (va chạm, NavMesh lỗi, hoặc vấn đề A nêu trên) mà `MovementController` dừng lại, `PatrolState` sẽ không bao giờ gọi lại lệnh di chuyển vì cờ `_isMovingToPoint` vẫn là `true`.

### C. Lỗi Reset Trạng thái khi dùng Object Pooling
- `EnemyBase` không xóa `_currentState` khi quái được đưa về Pool (`OnReturn`) hoặc khi lấy ra (`ResetEnemy`).
- **Hậu quả:** Quái mới có thể kế thừa trạng thái "đang bị khựng" hoặc trạng thái chết của quái cũ, dẫn đến logic `Update` bị sai ngay từ đầu.

## 3. Tech Stack & Boundaries
- **Tech Stack:** Unity 2022+, C#, NavMesh API, Rigidbody2D.
- **Boundaries:**
    - **Always:** Cập nhật logic reset trạng thái trong `EnemyBase`. Đồng bộ hóa các hằng số khoảng cách.
    - **Ask first:** Thay đổi kiến trúc FSM (nếu cần thiết hơn).
    - **Never:** Không dùng `Update` hay `Coroutine` lồng nhau để "force" di chuyển.

## 4. Proposed Solution (Giải pháp đề xuất)

### Phase 1: Đồng bộ hóa & Sửa lỗi Logic (Surgical Fix)
- Sửa `PatrolState` để đảm bảo cờ hiệu được reset hoặc lệnh di chuyển được duy trì cho đến khi đạt ngưỡng của State.
- Cập nhật `EnemyBase` để reset `_currentState` về `null` trong `OnReturn`.
- Đồng bộ hóa `PatrolReachDistance` giữa `EnemyBase` và `MovementController`.

### Phase 2: Nâng cao độ tin cậy (Robustness)
- Thêm cơ chế "Failsafe" vào `EnemyMovementController` để tự động Warp quái về NavMesh nếu bị văng ra ngoài.
- Thêm log cảnh báo nếu `MoveSpeed` của quái bị bằng 0.

## 5. Success Criteria (Tiêu chuẩn thành công)
1. Thief Enemy di chuyển ngay khi spawn.
2. Thief Enemy liên tục thay đổi điểm tuần tra khi đạt đích.
3. Khi Player vào tầm nhìn, Thief chuyển sang ChaseState và bám sát Player.
4. Không có lỗi NullReference liên quan đến `_currentState`.

## 6. Implementation Plan (Lộ trình thực hiện)
1. **Task 1:** Sửa `EnemyBase.cs` (Reset state & OnReturn).
2. **Task 2:** Sửa `PatrolState.cs` (Fix cờ hiệu & threshold).
3. **Task 3:** Sửa `EnemyMovementController.cs` (Warp logic & log).
4. **Task 4:** Kiểm tra thực tế trong Unity Editor.
