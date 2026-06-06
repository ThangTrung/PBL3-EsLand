# Plan: Drink Water Feature Implementation

## Phase 1: Scene & Infrastructure Setup
- [ ] **Step 1.1: Ensure Water Tilemap has Collider.**
  - Gán `TilemapCollider2D` (isTrigger = true) cho `Water_A` và các tilemap nước khác để có thể dùng `Physics2D.OverlapPoint`.
- [ ] **Step 1.2: Define Water Layer Mask.**
  - Kiểm tra và đảm bảo `Water` layer là layer số 4.

## Phase 2: Logic Implementation
- [ ] **Step 2.1: Update PlayerInteractionController.**
  - Thêm biến `[SerializeField] private LayerMask waterLayer;`.
  - Trong `HandleInteractionClick`, nếu không tìm thấy `interactableLayer`, hãy check `waterLayer`.
  - Nếu trúng nước, gọi hàm `MoveToAndDrink(mouseWorldPos)`.
- [ ] **Step 2.2: Implement Drinking Routine.**
  - Sử dụng Coroutine để: Di chuyển -> Đợi đến nơi -> Chạy Animation -> Cộng chỉ số Thirst.

## Phase 3: Testing
- [ ] **Step 3.1: Manual Test.**
  - Để nhân vật bị khát (giảm Thirst).
  - Click vào vùng nước.
  - Kiểm tra xem nhân vật có đi tới và tăng Thirst không.

## Tasks
- [ ] Task 1: Cấu hình Collider cho Tilemap Nước trong Scene.
- [ ] Task 2: Cập nhật `PlayerInteractionController.cs` để hỗ trợ uống nước.
