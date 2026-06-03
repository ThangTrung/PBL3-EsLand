# Spec: Map Gate System (Boss Death Unlock)

## Objective
Xây dựng cơ chế logic cho phép mở khóa đường đi (Cổng) ngay sau khi một Boss cụ thể bị tiêu diệt. Hệ thống phải đảm bảo tính ổn định, dễ cấu hình trong Inspector và lưu lại trạng thái đã mở để người chơi không phải đánh lại Boss.

## Tech Stack
- Unity 2022.3+
- C# (MonoBehaviour)
- Unity Save System (Tích hợp thông qua `ISaveable`)

## Commands
- Test: Chạy trực tiếp trong Editor (Play Mode), tiêu diệt Boss để kiểm tra sự kiện.

## Project Structure
- `Assets/Scripts/Gameplay/World/MapGateController.cs` -> Logic điều khiển việc ẩn/hiện vật cản.
- `Assets/Scripts/Gameplay/Characters/CharacterHealth.cs` -> Nguồn phát sự kiện `OnDie`.

## Code Style
- Sử dụng Event-driven (lắng nghe `OnDie`).
- Kiểm tra Null-reference chặt chẽ để tránh lỗi khi chưa gán Boss.
- Log thông tin rõ ràng trong Console để debug.

## Testing Strategy
1. Tạo một GameObject làm vật cản (Barrier).
2. Tạo một Boss giả lập có `CharacterHealth`.
3. Gán Boss và Vật cản vào `MapGateController`.
4. Giảm máu Boss về 0 -> Vật cản phải biến mất ngay lập tức.
5. Save game -> Thoát -> Load game -> Vật cản vẫn phải đang biến mất.

## Boundaries
- **Always:** Cung cấp `gateID` duy nhất cho mỗi cổng.
- **Always:** Gỡ bỏ lắng nghe sự kiện (`OnDie -= ...`) trong `OnDestroy` để tránh memory leak.
- **Never:** Không hardcode tên Boss hay ID cổng trong script.

## Success Criteria
- Sự kiện Boss chết được bắt chính xác.
- Vật cản (Gate Visuals/Colliders) được ẩn đi thành công.
- Trạng thái mở khóa được lưu vào file Save thành công.
