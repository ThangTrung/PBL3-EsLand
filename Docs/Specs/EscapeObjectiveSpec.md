# Spec: Escape Island Objective (Boat Crafting)

## Objective
Xây dựng cơ chế chiến thắng cuối cùng (End-game) cho trò chơi. Người chơi cần thu thập đủ 4 "Viên đá sức mạnh" (Power Stones) từ 4 con Boss khác nhau trên bản đồ để chế tạo một chiếc Thuyền. Khi thuyền được chế tạo và tương tác, người chơi sẽ thoát đảo và giành chiến thắng.

## Tech Stack
- Unity 2022.3+
- C# (MonoBehaviour, ScriptableObjects)
- Hệ thống Crafting & Inventory hiện có.

## Key Components

### 1. Boss Loot (Power Stones)
- **4 Loại đá:** Attack, Defense, Health, Speed (Đã có ScriptableObject trong `Assets/Resources/Items/PowerStone`).
- **Cơ chế rớt đồ:** Khi Boss chết, nó PHẢI rớt ra viên đá tương ứng với tỉ lệ 100%.

### 2. Boat Crafting
- **Recipe:** `Boat_Recipe.asset` (Đã có, yêu cầu 4 viên đá + Gỗ + Đá thường).
- **Phân loại:** Thuyền được coi là một loại công trình (`BuildingType.EscapeVehicle`).

### 3. Victory Logic (Escape)
- **EscapeBoatController:** Script mới gắn vào Prefab Thuyền. Triển khai `IInteractable`.
- **GameManager Victory:** Thêm hàm `HandleVictory()` vào `GameManager` để xử lý việc dừng game và hiện UI chiến thắng.

## Project Structure
- `Assets/Scripts/Gameplay/World/EscapeBoatController.cs` (New)
- `Assets/Scripts/Core/GameManager.cs` (Update)
- `Assets/Prefabs/Building/Boat/Boat.prefab` (Update)

## Code Style
- Tuân thủ `IInteractable` để người chơi có thể click vào thuyền sau khi chế tạo.
- Sử dụng Event-driven để thông báo chiến thắng.

## Success Criteria
- Mỗi Boss rớt đúng loại đá được gán.
- Chế tạo Thuyền thành công khi đủ nguyên liệu (4 viên đá + tài nguyên khác).
- Click vào Thuyền sau khi chế tạo sẽ hiện màn hình "Chiến Thắng" (Victory).
- Game dừng lại khi thắng.
