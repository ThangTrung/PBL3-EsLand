# Spec: Cooking Tower (Nhà Nướng Đồ Ăn)

## 1. Objective
Xây dựng tính năng "Lò nướng" (Cooking Tower) cho dự án game sinh tồn dựa trên prefab `Tower` có sẵn. Tính năng mô phỏng cơ chế lò nung của Minecraft: người chơi tương tác với lò nướng (khi đứng đủ gần), chuyển thực phẩm sống (Input) và nhiên liệu (Fuel - củi/than) từ túi đồ (Inventory) vào khe của lò. Sau một khoảng thời gian nung, thực phẩm sẽ chín và xuất ra khe thành phẩm (Output Slot).

**Cụ thể:**
1. **Highlight:** Khi người chơi đưa chuột vào `Tower`, đối tượng sẽ sáng lên.
2. **Interact:** Đứng gần và nhấn chuột phải vào `Tower` sẽ mở một UI Panel riêng của lò nướng (sử dụng asset Banner), đồng thời mở Inventory của người chơi.
3. **Chuyển vật phẩm:** Nhấn vào vật phẩm trong Inventory khi UI lò nướng đang mở sẽ chuyển vật phẩm đó vào lò (vào slot Input hoặc Fuel tương ứng).
4. **Nung nấu & Nhiên liệu:** Lò nướng sẽ tiêu thụ nhiên liệu theo thời gian (Fuel Burn Time) để nung nguyên liệu (Cooking Time).
5. **Thành phẩm:** Đồ ăn chín hiện ra ở khe Output, người chơi có thể click để lấy về túi đồ.

## 2. Tech Stack
- **Engine:** Unity 2D (Phiên bản theo dự án)
- **Language:** C#
- **Kiến trúc:** SOLID, MVP/MVC (Tách biệt Logic và UI), sử dụng ScriptableObject cho cấu hình công thức và vật phẩm.
- **Tài nguyên:** `Asset Tiny Swords` và `Tiny Swords old version`. Cụ thể dùng `banner` và `banner_slots` trong `Assets/Textures/Tiny Swords/ UI Elements/ Banners` cho UI Lò nung.

## 3. Project Structure
Dự kiến thêm/sửa các file sau:
- **Core/Contracts:**
  - `ICookingSystem` hoặc tương tự để quản lý logic.
- **Data (ScriptableObjects):**
  - `CookingRecipeSO`: ScriptableObject định nghĩa công thức nấu ăn (Input item, Output item, Cooking Time).
  - Có thể mở rộng `ItemSO` để chứa thông tin `FuelValue` (thời gian cháy của nhiên liệu).
- **Gameplay/Building:**
  - `CookingTower`: Component gắn vào Prefab Tower. Kế thừa `MonoBehaviour` và implement `IInteractable`. Kiểm tra khoảng cách khi tương tác, quản lý Input Slot, Fuel Slot, Output Slot, và logic hẹn giờ nung nấu.
- **UI:**
  - `CookingTowerUI`: Component quản lý hiển thị giao diện lò nướng. Dùng `banner` và `banner_slots`.
  - `CookingSlotUI`: Component hiển thị khe Input/Fuel/Output của lò.

## 4. Code Style & Architecture
- Không viết logic nấu nướng trong UI. UI chỉ lắng nghe event hoặc subscribe vào trạng thái của `CookingTower`.
- Sử dụng Event Action (`Action`, `Action<Item>`) để thông báo sự thay đổi trạng thái từ Logic sang UI.
- Sử dụng `EnvironmentHighlight` đã có trong dự án để xử lý highlight.
- Tính toán khoảng cách `Vector2.Distance(player, tower) < interactRange`.

Ví dụ định nghĩa CookingRecipeSO:
```csharp
[CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "Data/Cooking Recipe")]
public class CookingRecipeSO : ScriptableObject
{
    public ItemSO InputItem;
    public ItemSO OutputItem;
    public float CookingTime = 5f;
}
```

## 5. Testing Strategy
- Test qua Unity Editor (Play Mode).
- Mở Scene Test, thả Prefab `Tower` đã được cập nhật Component vào.
- Đứng gần/xa Tower, rê chuột kiểm tra Highlight và Click phải kiểm tra mở UI (kiểm chứng giới hạn khoảng cách).
- Click chuột phải mở UI Lò nung (sử dụng Banner) và Inventory.
- Chuyển nhiên liệu và thức ăn sống vào lò. Kiểm tra thanh Fuel và Progress nấu.
- Kiểm tra Output sinh ra thức ăn chín và lấy về Inventory thành công.

## 6. Lộ trình chi tiết (Tasks Breakdown)
- **Phase 1: Setup Logic (Data & Gameplay)**
  - Tạo/cập nhật class cấu hình Data: `CookingRecipeSO` và thuộc tính nhiên liệu cho `ItemSO`.
  - Tạo class `CookingTower` gắn vào Prefab `Tower`. Implement `IInteractable` cho tương tác (click phải) và giới hạn khoảng cách.
  - Implement logic Minecraft Furnace (Fuel Burn, Cooking Progress) trong `CookingTower`.
- **Phase 2: Xây dựng UI Lò Nướng**
  - Tạo Prefab UI Lò nướng (`CookingTowerPanel`) dùng assets banner.
  - Gồm 3 ô (Input, Fuel, Output), thanh Fuel cháy và thanh Progress nung.
  - Viết `CookingTowerUI` script để liên kết giao diện với data từ `CookingTower`.
- **Phase 3: Kết nối Inventory và Tương tác**
  - Quản lý đóng/mở cùng lúc Inventory và CookingTower UI.
  - Xử lý sự kiện click trên Inventory để tự động đưa đúng loại Item (thịt -> Input, than -> Fuel) sang lò.
  - Xử lý click ở Output Slot để nhận thành phẩm.

## 7. Open Questions (Đã được trả lời)
- [x] Cần hệ thống nhiên liệu (Fuel).
- [x] Có giới hạn khoảng cách tương tác (đứng gần mới mở được).
- [x] Dùng asset `banner` và `banner_slots` của Tiny Swords.

---
*Vui lòng xác nhận các thông tin trên và trả lời Open Questions trước khi chúng ta chuyển sang bước lập kế hoạch (Plan).*