# Plan: Sửa lỗi di chuyển Thief Enemy

## Overview
Kế hoạch này tập trung vào việc sửa chữa logic điều khiển di chuyển và quản lý vòng đời của Enemy trong hệ thống Object Pooling.

## Tasks

### 1. Cải thiện Vòng đời Enemy (EnemyBase.cs)
- [ ] Reset `_currentState` về `null` trong `OnReturn`.
- [ ] Chỉnh sửa `SetInitialState` để luôn ép buộc trạng thái `PatrolState` khi reset quái.
- [ ] Thêm log kiểm tra `ConfigInternal` khi `InitializeEnemy` được gọi.

### 2. Sửa lỗi Logic Tuần tra (PatrolState.cs)
- [ ] Xóa bỏ cờ `_isMovingToPoint` hoặc thay thế bằng logic kiểm tra trạng thái di chuyển từ `MovementController`.
- [ ] Cập nhật `Execute` để gọi lại lệnh di chuyển nếu quái bị dừng đột ngột mà chưa tới đích.
- [ ] Đảm bảo `PatrolReachDistance` được truyền vào `MoveTowardsPosition` một cách nhất quán.

### 3. Đồng bộ hóa Controller (EnemyMovementController.cs)
- [ ] Thêm thuộc tính `IsNavigating` công khai để State có thể truy vấn.
- [ ] Sửa đổi `FixedUpdate` để xử lý mượt mà hơn khi quái bị lệch khỏi NavMesh.
- [ ] Thêm `Debug.LogWarning` nếu `baseMoveSpeed` được set về 0 để dễ debug trong Editor.

### 4. Verification (Kiểm chứng)
- [ ] Spawn Thief trong Scene Test.
- [ ] Kiểm tra Console xem có log lỗi không.
- [ ] Quan sát hành vi tuần tra trong ít nhất 3 chu kỳ (3 điểm khác nhau).

## Risks & Mitigations
- **Risk:** Quái có thể bị "giật" (jitter) nếu State gọi lệnh di chuyển quá thường xuyên.
- **Mitigation:** Đảm bảo `MovementController` có cơ chế `pathUpdateCooldown` (đã có sẵn trong code hiện tại).
