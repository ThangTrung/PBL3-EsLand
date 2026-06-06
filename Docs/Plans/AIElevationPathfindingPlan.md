# Plan: AI Elevation Pathfinding Implementation

## Phase 1: Walkable Surface Infrastructure
- [ ] **Step 1.1: Add Colliders to Stairs.**
  - Gán `TilemapCollider2D` cho `Stairs_A_to_B`, `Stairs_B_to_C`.
  - Set Layer của cầu thang về layer `Land` (Layer 11) để thống nhất quản lý vùng đi bộ.
- [ ] **Step 1.2: Add Colliders to Land.**
  - Đảm bảo `Land_A`, `Land_B`, `Land_C` đều có `TilemapCollider2D`.

## Phase 2: NavMesh Manager Optimization
- [ ] **Step 2.1: Configure NavMeshSurface.**
  - Đổi `Use Geometry` thành `Physics Colliders`.
  - Thiết lập `Layer Mask` chỉ lấy layer `Land` và `Default` (để tránh lấy nhầm các vật cản làm đường đi).
- [ ] **Step 2.2: Puncture Cliffs Blocker.**
  - Kiểm tra và xóa bỏ các ô gạch trên Tilemap `Cliffs_Blocker` tại các vị trí cầu thang để "mở đường" cho AI.

## Phase 3: Validation & Baking
- [ ] **Step 3.1: Re-bake NavMesh.**
  - Thực hiện lệnh Bake trên `NavMesh Manager` để cập nhật bản đồ mới.
- [ ] **Step 3.2: Integration Test.**
  - Dụ quái chạy từ Elevation_A lên Elevation_C và ngược lại.

## Tasks
- [ ] Task 1: Cấu hình Collider và Layer cho hệ thống nền đất/cầu thang.
- [ ] Task 2: Tối ưu hóa NavMesh Manager và thực hiện Bake lại bản đồ AI.
- [ ] Task 3: Kiểm tra tính thông suốt của đường đi giữa các tầng.
