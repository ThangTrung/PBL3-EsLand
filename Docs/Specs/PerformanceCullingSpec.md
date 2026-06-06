# Spec: Performance Culling System (Distance-based)

## Objective
Khắc phục hiện tượng lag (giảm FPS) khi bản đồ có quá nhiều vật thể (cây, đá, công trình). Cơ chế này sẽ tạm thời vô hiệu hóa hiển thị (Renderer) và vật lý (Collider) của các vật thể ở quá xa người chơi, giúp giảm tải cho cả CPU và GPU.

## Tech Stack
- Unity 2022.3+
- C# (MonoBehaviour)
- Kỹ thuật: Staggered Update (xử lý chia nhỏ theo frame) & SqrMagnitude.

## Key Components

### 1. CullableEntity.cs
- Gắn vào các Prefab cần tối ưu (Cây, Đá, Quái, Item rớt).
- Tự động nhận diện các component cần tắt: `Renderer`, `Collider2D`, `Animator`, `MonoBehaviour` (logic).
- Có trạng thái `IsCulled`.

### 2. PerformanceManager.cs (Singleton)
- Danh sách quản lý tất cả `CullableEntity`.
- **Culling Radius:** Bán kính hoạt động (mặc định 25-30m).
- **Check Interval:** Tần suất kiểm tra (mặc định 0.5s).
- **Staggered Logic:** Thay vì kiểm tra 24,000 vật thể trong 1 frame, nó sẽ chia ra xử lý khoảng 500-1000 vật thể mỗi frame để tránh gây khựng hình (spike).

## Project Structure
- `Assets/Scripts/Infrastructure/Performance/CullableEntity.cs`
- `Assets/Scripts/Infrastructure/Performance/PerformanceManager.cs`

## Success Criteria
- FPS tăng đáng kể khi chạy trên map rộng.
- Các vật thể xuất hiện/biến mất mượt mà khi người chơi di chuyển lại gần/ra xa.
- Không gây ra lỗi logic (ví dụ: vật thể bị biến mất vĩnh viễn hoặc không thể tương tác khi lại gần).
