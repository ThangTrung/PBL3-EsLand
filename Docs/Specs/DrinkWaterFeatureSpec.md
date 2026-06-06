# Spec: Drink Water Feature

## Objective
Cho phép người chơi uống nước để hồi phục chỉ số Khát (Thirst) bằng cách click chuột trái vào các vùng nước trên bản đồ.

## Mechanics
1. **Phát hiện Nước:** Khi người chơi click chuột trái, hệ thống sẽ kiểm tra xem vị trí click có thuộc Layer `Water` (Layer 4) hay không.
2. **Di chuyển:** Nếu click vào nước, nhân vật sẽ tự động tìm đường (NavMesh) đi tới điểm đó.
3. **Tương tác:** Khi đến đủ gần (khoảng cách tương tác), nhân vật sẽ dừng lại và thực hiện hành động "Uống nước" (chạy animation `interact`).
4. **Hồi phục:** Sau một khoảng trễ ngắn (mô phỏng hành động uống), chỉ số Khát trong `PlayerSurvivalController` sẽ được cộng thêm (mặc định +20).

## Tech Stack
- Unity 2022.3+
- C# (MonoBehaviour)
- NavMeshPlus (Cho di chuyển 2D)
- Physics2D (Để phát hiện Tilemap Nước)

## Project Structure
- `Assets/Scripts/Gameplay/Characters/PlayerInteractionController.cs` (Cập nhật logic click)
- `Assets/Scripts/Gameplay/Characters/PlayerSurvivalController.cs` (Cập nhật hàm AddThirst nếu cần)
- Cấu hình Scene: Thêm `TilemapCollider2D` vào các Tilemap thuộc layer `Water`.

## Success Criteria
- Click vào nước -> Nhân vật đi tới mép nước.
- Đến nơi -> Nhân vật cúi xuống (animation) -> Chỉ số khát tăng lên.
- Click vào vật thể khác (Cây, Đá) vẫn hoạt động bình thường.
