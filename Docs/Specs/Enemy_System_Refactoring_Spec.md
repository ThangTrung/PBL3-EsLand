# Spec: Enemy System Refactoring & Stabilization

## Objective
Khắc phục triệt để lỗi "Enemy bị đứng yên sau khi nhận sát thương", đồng thời chuẩn hóa kiến trúc Prefab của Enemy (tách biệt Visual và Logic) tương tự như Player (`Pawn_black`). Việc này giúp hệ thống Enemy tuân thủ nguyên lý SOLID, dễ dàng mở rộng và không bị xung đột vật lý/animation.

## Root Cause Analysis (Chuẩn đoán bệnh)
1. **Lỗi NavMeshAgent & Scale Flipping:** Hiện tại `EnemyMovementController` đang lật (flip) toàn bộ Root Transform của Enemy (scale X = -1). Việc lật Root chứa `NavMeshAgent` và `Collider` gây ra lỗi hỏng dữ liệu không gian của NavMesh, khiến Agent bị kẹt (đứng yên vĩnh viễn).
2. **Xung đột State Machine & Knockback:** `CombatFeedbackController` xử lý Knockback bằng một Coroutine và can thiệp thô bạo vào `EnemyMovementController` (gọi `SetCanMove(false)`). Tuy nhiên, State Machine lúc này vẫn đang ở `ChaseState` và liên tục gọi `FollowTarget`, dẫn đến việc spam lệnh di chuyển đè lên lệnh Knockback, gây giật lag và kẹt State.
3. **Thiếu Hit/Hurt State (SOLID Violation):** AI không có một State chính thức để xử lý việc bị choáng/đẩy lùi. Logic bị phân mảnh giữa `EnemyBase` và `CombatFeedbackController`.

## Tech Stack
- Unity 2D (C#)
- Object Pooling
- NavMeshAgent (2D)
- Design Patterns: State Machine (FSM), Strategy, Factory.

## Project Structure (Quy hoạch thư mục)
```
Assets/
├── Scripts/Gameplay/AI/
│   ├── States/
│   │   ├── HitState.cs         → (NEW) Trạng thái xử lý Knockback/Stun
│   │   ├── PatrolState.cs      → Cập nhật
│   │   ├── ChaseState.cs       → Cập nhật
│   ├── EnemyBase.cs            → Cập nhật tham chiếu
│   ├── Movement/
│   │   └── EnemyMovementController.cs → Sửa logic lật hình (flip)
├── Scripts/Gameplay/Combat/Feedback/
│   └── CombatFeedbackController.cs    → Xóa logic SetCanMove, chỉ giữ visual/physics force
└── Prefabs/Characters/Enemies/        → Cấu trúc lại toàn bộ prefabs
```

## Code Style
- **Tách biệt Visual/Logic:** Root chứa Logic (Rigidbody, NavMesh, Controller). Child `Visual` chứa hiển thị (SpriteRenderer, Animator).
- **SOLID FSM:** State Machine quản lý việc có được phép di chuyển hay không, thay vì để Coroutine bên ngoài can thiệp.

## Testing Strategy
- PlayMode Tests: Gọi `TakeDamage` liên tục vào Enemy để đảm bảo không bị kẹt.
- Manual Verification: Spawn Enemy từ `EnemySpawnDirector`, tấn công thử bằng vũ khí của Player để kiểm tra Knockback và việc quay trở lại `ChaseState` mượt mà.

## Boundaries
- **Always:** Cập nhật mọi Enemy Prefab hiện có sang cấu trúc mới (tách child `Visual`). Lật hình (flip scale) chỉ thực hiện trên `Visual`.
- **Ask first:** Nếu có Enemy nào dùng chung SpriteRenderer trên root cho một Logic đặc thù ngoài quy chuẩn.
- **Never:** Tuyệt đối KHÔNG lật `transform.localScale` của Root GameObject chứa Rigidbody/NavMeshAgent.

## Success Criteria
1. Tấn công Enemy liên tục không làm Enemy bị kẹt hoặc đứng im vĩnh viễn.
2. Knockback hoạt động mượt mà, Enemy văng lùi lại và sau đó tự động đuổi theo Player.
3. Toàn bộ Enemy Prefabs có cấu trúc: Root (Logic) -> Child `Visual` (Hiển thị).
4. `NavMeshAgent` không bị lỗi do scale âm.
5. AI FSM được bổ sung `HitState`.

## Phases of Implementation (Kế hoạch thực thi)

### Phase 1: Prefab Architecture & Component Referencing
- Cập nhật `EnemyBase` và `Character` để lấy `CharacterAnimationController` và `SpriteRenderer` từ con (`GetComponentInChildren`).
- Sửa `EnemyMovementController` và `CharacterAnimationController` để chỉ Flip object `Visual` thay vì Flip Root.

### Phase 2: SOLID FSM - Introduce HitState
- Tạo `HitState.cs` implementing `IAIState`.
- Sửa `EnemyBase.HandleDamageTaken`: Khi nhận sát thương, tự động chuyển sang `HitState` (hoặc `DefenseState` nếu roll trúng).
- Sửa `CombatFeedbackController`: Xóa bỏ việc ép `EnemyMovementController.SetCanMove(false)`, chỉ đơn thuần add `ForceMode2D.Impulse` để văng ra.

### Phase 3: Update All Enemy Prefabs
- Tạo script Unity Editor (hoặc sử dụng MCP batch_execute) để tự động hóa việc tạo object `Visual` cho toàn bộ các Prefab Enemy trong `Assets/Prefabs/Characters/Enemies/`.
- Di chuyển `SpriteRenderer`, `CharacterAnimationController` vào trong `Visual`. Đặt scale của Root về `(1,1,1)`.

---
*Ghi chú: Việc chia Phase này giúp đảm bảo code không bị lỗi dây chuyền và dễ dàng review nghiệm thu sau mỗi bước.*