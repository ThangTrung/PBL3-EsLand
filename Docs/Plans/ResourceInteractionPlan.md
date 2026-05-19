# Implementation Plan - Resource Interaction

Tính năng: Player tự động di chuyển đến và tương tác với Tài nguyên (Cây, Đá) khi click chuột.

## 1. Thành phần chính (Components)
- **PlayerInputController:** Nhận click chuột, Raycast tìm target `IInteractable`.
- **PlayerInteractionController:** Quản lý logic tương tác, ra lệnh cho MovementController di chuyển tới target và thực hiện đòn đánh.
- **PlayerMovementController:** Xử lý di chuyển đường thẳng tới vị trí target và dừng lại khi chạm collider.
- **ResourceNode / TreeResource:** Chứa thông tin máu, Tool yêu cầu, và logic nhận sát thương.
- **ResourceVisualEffects:** Lắng nghe event `OnDamaged` từ `ResourceNode` để thực hiện hiệu ứng rung (Shake).

## 2. Thứ tự thực hiện (Implementation Order)
1.  **Refactor ResourceNode & TreeResource:** Đảm bảo logic `TakeDamage` gọi đầy đủ các event và trigger animator.
2.  **Cập nhật PlayerMovementController:** Đảm bảo hàm `SetFollowTarget` hoạt động ổn định với kiểm tra va chạm (Touching).
3.  **Cập nhật PlayerInteractionController:** Hoàn thiện hàm `InteractWithTarget` để điều phối chuỗi hành động: Di chuyển -> Quay mặt -> Đánh -> Tương tác logic.
4.  **Kiểm tra PlayerInputController:** Đảm bảo việc click chuột ưu tiên chọn target là tài nguyên trước khi thực hiện đánh thường.

## 3. Rủi ro và Giải pháp (Risks & Mitigation)
- **Rủi ro:** Player bị "kẹt" khi cố di chuyển tới tâm của cây mà không chạm được Collider (do offset).
- **Giải pháp:** Sử dụng `myCollider.IsTouching(targetCollider)` trong `PlayerMovementController` thay vì kiểm tra khoảng cách tới tâm, giúp dừng lại chính xác ngay khi chạm mép vật thể.
- **Rủi ro:** Hiệu ứng Shake bằng code (`ResourceVisualEffects`) xung đột với Animator.
- **Giải pháp:** `ResourceVisualEffects` thay đổi `localPosition`, trong khi Animator nên điều khiển các thuộc tính khác hoặc layer khác nếu cần. Khuyên dùng Shake bằng code để linh hoạt hơn.

## 4. Điểm kiểm chứng (Verification Checkpoints)
- [ ] Script biên dịch không lỗi.
- [ ] Click vào cây -> Player di chuyển tới.
- [ ] Player dừng lại ngay khi chạm cây -> Animator Player chạy Trigger "interact".
- [ ] Cây rung lên (Shake) và máu giảm (Check Inspector).

---

## 5. Hướng dẫn cấu hình Unity Editor (Dành cho User)

Bạn vui lòng thực hiện các bước sau trên Unity Editor để hệ thống hoạt động:

### Bước 1: Thiết lập Layer và Tag
1. Vào `Edit > Project Settings > Tags and Layers`.
2. Tạo một Layer mới tên là **Interactable** (nếu chưa có).
3. Gán Layer **Interactable** cho các Prefab Cây (`Tree`) và Đá (`Rock`).

### Bước 2: Cấu hình Prefab Tài nguyên (Cây, Đá)
1. Mở Prefab Cây/Đá.
2. Đảm bảo có component **BoxCollider2D** hoặc **CircleCollider2D** (đã bật `Is Trigger = False` để có thể va chạm vật lý).
3. Thêm component **ResourceNode** (hoặc **TreeResource** cho cây).
4. Thêm component **ResourceVisualEffects** (để có hiệu ứng rung khi bị đánh).
5. (Nếu dùng Animator) Đảm bảo Animator Controller có một Trigger tên là **Hit**.

### Bước 3: Cấu hình Prefab Player
1. Chọn Player trong Scene hoặc Prefab.
2. Tại component **PlayerInteractionController**:
   - Gán `Interactable Layer` là Layer **Interactable** bạn vừa tạo ở Bước 1.
   - Chỉnh `Interaction Range` (tầm đánh) phù hợp (ví dụ: 1.5).
3. Đảm bảo Player có **Rigidbody2D** (Body Type = Dynamic, Collision Detection = Continuous, Freeze Rotation Z = True) và một **Collider2D** để nhận diện va chạm với cây.

### Bước 4: Kiểm tra Animator Player
1. Mở Animator của Player.
2. Đảm bảo có một Trigger tên là **interact** để chạy animation chặt cây/khai thác đá.
