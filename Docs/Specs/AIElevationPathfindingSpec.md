# Spec: AI Elevation Pathfinding Fix

## Objective
Khắc phục triệt để lỗi kẻ địch (Enemies) không thể đuổi theo người chơi khi thay đổi tầng cao độ (Elevation). Đảm bảo hệ thống NavMesh là một mạng lưới liên tục, cho phép AI tìm đường đi qua cầu thang và tự động kích hoạt chuyển đổi layer hình ảnh.

## 1. Phân tích nguyên nhân
- **Mất kết nối NavMesh:** Các Tilemap Cầu thang (`Stairs_A_to_B`, v.v.) hiện không có Collider, dẫn đến việc hệ thống NavMesh không nhận diện được "cây cầu" nối giữa 2 tầng đất.
- **Vật cản đè lên đường đi:** `Cliffs_Blocker` (Vách đá) có thể đang đè lên phần cầu thang, làm NavMesh tại đó bị đánh dấu là "Not Walkable".
- **Cấu hình Baking chưa tối ưu:** `NavMesh Manager` đang sử dụng `RenderMeshes` để bake, vốn không hoạt động tốt với Tilemap, dẫn đến các lỗ hổng trên bản đồ AI.

## 2. Giải pháp toàn cục
### 2.1. Hạ tầng va chạm (Walkable Surface)
- Tất cả các Tilemap mang tính chất "nền đất" hoặc "cầu thang" (`Land_A`, `Land_B`, `Stairs_...`) phải có component **`TilemapCollider2D`**.
- Các Collider này sẽ được đánh dấu là **`Used by Composite`** để tối ưu hóa va chạm nếu cần, nhưng quan trọng nhất là để NavMesh có thể "nhìn thấy" chúng.

### 2.2. Cấu hình NavMesh Manager
- Đổi `Use Geometry` từ `RenderMeshes` sang **`Physics Colliders`**.
- Đảm bảo `Layer Mask` thu thập nguồn bao gồm tất cả các layer: `Land`, `Default` (cho cầu thang), và `Interactable`.

### 2.3. Tối ưu hóa AI Movement
- Cập nhật `EnemyMovementController` để đảm bảo `NavMeshAgent` luôn bám sát vị trí thực tế của nhân vật (Fix lỗi trượt NavMesh).

## 3. Success Criteria
- Kẻ địch có thể tự động tìm đường đi lên/xuống cầu thang khi đuổi theo Player.
- Khi đi qua cầu thang, kẻ địch tự động kích hoạt `ElevationGateway` và đổi Sorting Layer chính xác.
- Không còn hiện tượng AI đứng khựng lại ở chân cầu thang hoặc vách đá.
