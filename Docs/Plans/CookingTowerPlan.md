# Implementation Plan: Cooking Tower

## 1. Components & Dependencies
**Data Layer:**
- `CookingRecipeSO`: ScriptableObject để định nghĩa các công thức nấu ăn.
- `ItemSO` (Sửa đổi): Cần thêm trường `FuelTime` (thời gian cháy của nhiên liệu, mặc định = 0 nếu không phải nhiên liệu).

**Logic Layer:**
- `CookingTower`: Gắn vào `Tower` prefab. Quản lý trạng thái (Đang nấu, Dừng nấu), chứa dữ liệu item (Input, Fuel, Output), và đếm ngược thời gian nấu. Cần có method để Inventory chuyển item vào.
- `ICookingSystem` (Interface) hoặc events bên trong `CookingTower` (`Action OnCookingStateChanged`, `Action<float> OnProgressChanged`, `Action<float> OnFuelChanged`).

**UI Layer:**
- `CookingTowerUI`: Gắn vào Prefab UI (Panel Lò nướng).
- `CookingSlotUI`: UI cho các khe (tương tự như `InventorySlotUI` nhưng tuỳ chỉnh cho Lò).
- Thanh progress (Slider hoặc Image fill) cho Fuel và Cooking Progress.

**Dependencies:**
- Tính năng sẽ dựa vào hệ thống `ItemSO` hiện tại của game.
- Giao diện phụ thuộc vào hệ thống quản lý Canvas/Panel hiện tại (để đóng/mở cùng lúc với Inventory).

## 2. Implementation Order (Thứ tự triển khai)

**Step 1: Data Setup (Bắt buộc làm trước)**
- Tạo file `CookingRecipeSO.cs`.
- Mở rộng `ItemSO.cs` thêm `FuelTime` (hoặc tạo một `ItemCategory.Fuel`).
- Tạo một vài scriptable object mẫu để test: Gỗ (Nhiên liệu), Thịt Sống (Input), Thịt Chín (Output) và 1 Recipe tương ứng.

**Step 2: Core Logic (CookingTower.cs)**
- Viết script `CookingTower.cs`, kế thừa `MonoBehaviour` và implement `IInteractable` (nếu dự án dùng Interface này cho click chuột phải).
- Cài đặt hàm `Interact()` có check khoảng cách `Vector2.Distance`.
- Thêm thuộc tính lưu 3 Slot: `InputItem`, `FuelItem`, `OutputItem` (và số lượng nếu có stack).
- Viết hàm `Update()` xử lý giảm Fuel Time và tăng Cooking Time.
- Chạy Unit Test hoặc Test thử Component trong Editor bằng Debug.Log.

**Step 3: UI Layout & Scripts**
- Bật Canvas, dùng `banner` và `banner_slots` tạo Prefab `CookingTowerPanel`.
- Viết `CookingSlotUI.cs` (có hình ảnh item, số lượng).
- Viết `CookingTowerUI.cs`. Inject reference của `CookingTower` vào `CookingTowerUI` để UI đọc dữ liệu (Progress, Fuel, Items) và cập nhật hiển thị.
- Đăng ký (Subscribe) các Action/Event từ `CookingTower` sang `CookingTowerUI`.

**Step 4: Integration with Inventory**
- Cập nhật UI Controller để khi mở `CookingTowerUI` thì `InventoryPanelUI` cũng bật lên.
- Gắn logic "Click vào Item trong Inventory": Khi `CookingTowerUI` đang mở, click vào `InventorySlotUI` thay vì Use/Drop thì sẽ check xem item đó có phải là nguyên liệu (Input) hay nhiên liệu (Fuel) không. Nếu đúng, đẩy sang `CookingTower`.
- Tương tự, click vào Slot ở `CookingTowerUI` thì đẩy Item ngược về Inventory.

## 3. Risks & Mitigations
- **Risk:** Làm sao biết được khi click trong Inventory là đang tương tác với Lò Nướng hay Tương tác bình thường?
- **Mitigation:** Dùng `GameState` hoặc cờ `IsCookingPanelOpen` trong GameManager/UIManager. Hoặc truyền Context `IItemActionHandler` vào Inventory.

## 4. Checkpoints
- [ ] Checkpoint 1: ScriptableObject và Data hoạt động. (Verify bằng Inspector)
- [ ] Checkpoint 2: Logic `CookingTower` chạy ổn trong console (in ra Debug.Log tiến trình nấu).
- [ ] Checkpoint 3: UI Lò Nướng hiện lên, hiển thị đúng các thanh thời gian/số lượng.
- [ ] Checkpoint 4: Người chơi có thể click trao đổi vật phẩm giữa Inventory và Lò, Lò nướng ra thành phẩm và lấy về.

---
*Kế hoạch này đã sẵn sàng để chuyển thành các Task cụ thể (Phase 3) nếu bạn đồng ý. Bạn có muốn duyệt qua plan này trước khi tôi tạo các Tasks chi tiết không?*