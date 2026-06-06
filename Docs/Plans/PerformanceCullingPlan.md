# Plan: Performance Culling Implementation

## Overview
Xây dựng hệ thống quản lý vòng đời hiển thị dựa trên khoảng cách để tối ưu hóa 24,000+ vật thể trong Scene.

## Implementation Order

### 1. Infrastructure Setup
- Tạo thư mục `Assets/Scripts/Infrastructure/Performance/`.
- Viết `CullableEntity.cs`: Logic bật/tắt component.
- Viết `PerformanceManager.cs`: Logic quản lý danh sách và vòng lặp Staggered.

### 2. Integration & Automation
- Tích hợp `CullableEntity` vào các Base class nếu được (như `ResourceNode` hoặc `EnemyBase`).
- **Hoặc:** Dùng MCP quét và add `CullableEntity` vào tất cả các Prefab tài nguyên và quái vật.

### 3. Testing & Calibration
- Test FPS trước và sau khi kích hoạt.
- Điều chỉnh `CullingRadius` sao cho người chơi không thấy vật thể "hiện hình" quá đột ngột ở mép màn hình.

## Tasks
- [ ] Task 1: Thực hiện script `CullableEntity` và `PerformanceManager`.
- [ ] Task 2: Gán `CullableEntity` cho các Prefab chính (Cây, Đá, Quái).
- [ ] Task 3: Kiểm tra hiệu quả giảm lag trong Scene thực tế.
