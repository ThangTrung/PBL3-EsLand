# Spec: Boss Movement Optimization (Fix Stuttering)

## Objective
Khắc phục triệt để hiện tượng Boss di chuyển bị giật, khựng (jitter/stutter) khi sử dụng NavMeshAgent kết hợp Rigidbody2D. Đảm bảo Boss di chuyển mượt mà, "lướt" trên địa hình và có quán tính/góc cua tự nhiên của một thực thể lớn.

### User Stories / Acceptance Criteria
- Boss không được khựng lại khi NavMesh đang tính toán đường đi (pathPending).
- Boss không bị giật li ti (micro-jitter) do xung đột vị trí giữa NavMeshAgent và Rigidbody2D.
- Boss phải có góc cua mượt (Steering Smoothing), không xoay gắt 90 độ.
- Boss không bị nháy (flickering) giữa trạng thái Idle và Run khi ở sát mục tiêu (Hysteresis).

## Tech Stack
- Unity 2022+ / 6.
- C# (.NET Standard 2.1).
- Unity NavMesh (AI Navigation).
- Physics 2D (Rigidbody2D Interpolated).

## Commands
- **Build/Compile:** Thực hiện tự động trong Unity Editor sau khi lưu script.
- **Test:** Chạy Scene `[Test_Boss_Movement]` hoặc Observe trực tiếp trong Play Mode.

## Project Structure
- `Assets/Scripts/Gameplay/AI/Movement/`: Chứa các Controller và Strategy di chuyển.
- `Assets/Scripts/Gameplay/AI/States/`: Chứa logic điều khiển FSM (Chase, Patrol).

## Code Style
- Tuân thủ SOLID.
- Sử dụng Strategy Pattern cho các loại di chuyển khác nhau.
- Ưu tiên tính toán trong `FixedUpdate` cho các tác vụ liên quan đến vật lý.

## Testing Strategy
- **Manual Test:** Quan sát quỹ đạo di chuyển của Boss trong Scene View với Gizmos bật.
- **Edge Cases:** Kiểm tra khi mục tiêu di chuyển nhanh, khi Boss đi vào góc hẹp, và khi Boss bị đẩy lùi (Knockback).

## Boundaries
- **Always:** Sử dụng `FixedUpdate` cho Rigidbody, bật `Interpolate`.
- **Ask first:** Thay đổi cấu hình `NavMeshAgent` mặc định trên Prefab.
- **Never:** Sử dụng `transform.position` trực tiếp khi có Rigidbody; không hard-code vận tốc.

## Success Criteria
- [ ] Boss di chuyển liên tục, không có frame nào vận tốc = 0 trừ khi đã tới đích.
- [ ] Độ lệch giữa Agent.nextPosition và Transform.position luôn < 0.1 đơn vị.
- [ ] Góc xoay (Facing) thay đổi dựa trên vận tốc thực tế, có độ trễ mượt.

## Open Questions
1. Bạn có muốn Boss có khả năng né tránh các chướng ngại vật động (Local Avoidance) gắt hơn không, hay ưu tiên sự mượt mà của quỹ đạo?
2. Có cần hỗ trợ các loại Boss bay (Flying Boss) trong tương lai với cùng Controller này không?
