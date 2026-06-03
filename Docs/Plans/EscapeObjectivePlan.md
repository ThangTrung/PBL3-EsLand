# Plan: Escape Objective Implementation

## Overview
Triển khai hệ thống mục tiêu cuối cùng: Thu thập đá từ Boss -> Chế thuyền -> Thoát đảo.

## Implementation Order

### Phase 1: Boss Loot System
1. **Fix Enemy Drop Logic:** Cập nhật `Enemy.cs` để thực sự spawn vật phẩm rớt ra (sử dụng logic tương tự `LootSpawner`).
2. **Setup Boss Prefabs:** Hướng dẫn người chơi gán đúng loại đá vào `lootTable` của từng Boss trong Inspector.

### Phase 2: Escape Logic
1. **Create EscapeBoatController:**
   - Triển khai `IInteractable`.
   - Hàm `Interact()` sẽ gọi `GameManager.Instance.Victory()`.
2. **Update Boat Prefab:**
   - Gắn `EscapeBoatController`.
   - Gắn `BoxCollider2D` (isTrigger) và set Layer là `Interactable`.
   - Gắn `Highlight` component để người chơi biết có thể tương tác.

### Phase 3: Victory UI
1. **Update GameManager:** Thêm sự kiện hoặc hàm xử lý khi thắng cuộc.
2. **Simple Victory UI:** Tạo một UI đơn giản (Text + Button Restart) hiện lên khi thắng.

## Tasks
- [ ] Task 1: Cập nhật `Enemy.cs` để hỗ trợ rớt đồ thực tế.
- [ ] Task 2: Tạo script `EscapeBoatController.cs`.
- [ ] Task 3: Cấu hình Prefab Thuyền (Boat).
- [ ] Task 4: Tích hợp logic Victory vào `GameManager` và UI.
