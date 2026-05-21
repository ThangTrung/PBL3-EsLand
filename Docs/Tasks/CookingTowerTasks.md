# Tasks: Cooking Tower (Lò nướng đơn giản)

Dựa trên nguyên tắc tối giản (Minecraft) và **Tối đa hóa tính tái sử dụng**:
- **Fuel:** Dùng chung hệ thống `ItemData` (thêm thuộc tính `FuelTime`).
- **Recipe:** **TÁI SỬ DỤNG** thuộc tính `canBeSmelted`, `resultItem`, `smeltTime` đã có sẵn trong `MaterialItem`. **Không cần** tạo mới `CookingRecipeSO`.
- **UI:** **TÁI SỬ DỤNG** `InventorySlotUI` hiện có của dự án. Không cần viết class mới hiển thị slot.

---

## Task 1: Cập nhật Data (Tái sử dụng ItemSO & MaterialItem)
- [ ] Mở script `ItemData.cs` và thêm `[SerializeField] private float fuelTime = 0f;` và property `public float FuelTime => fuelTime;`
- [ ] Mở rộng `InventorySlotUI.cs` để hỗ trợ Left Click (Tạo thêm event `OnLeftClicked` hoặc `OnSlotClicked` để Lò nướng dễ dàng bắt sự kiện click rút/thêm item).
  - *Acceptance:* `ItemData` có thông số fuel. `InventorySlotUI` có thể bắn event khi left click.
  - *Verify:* Kiểm tra code không báo lỗi, xem trong Inspector.
  - *Files:* `ItemData.cs`, `InventorySlotUI.cs`.

## Task 2: Core Logic của Lò Nướng (`CookingTower.cs`)
- [ ] Tạo `CookingTower.cs` và gắn vào `Tower.prefab`.
- [ ] Implement `IInteractable` (Check khoảng cách `Vector2.Distance(player, tower)`).
- [ ] **TÁI SỬ DỤNG** class `InventorySlot` hiện có: Khởi tạo mảng `InventorySlot[3]` cho Input (0), Fuel (1), Output (2).
- [ ] Viết hàm `Update()`:
  - Nếu có Input (`MaterialItem` có `canBeSmelted=true`) và có `currentFuelTime > 0` -> tăng `cookingProgress`.
  - Nếu `currentFuelTime <= 0` nhưng có Input và có Fuel trong slot (1) -> Tiêu thụ 1 Fuel, lấy `fuelTime` cộng vào `currentFuelTime`.
  - Nếu `cookingProgress` >= `smeltTime` -> Trừ 1 Input, thêm 1 Output (`resultItem`), reset progress.
  - *Acceptance:* Logic hoạt động đúng (thử gọi các hàm nạp item bằng script test hoặc xem giá trị chạy trên Inspector).
  - *Verify:* Kéo Prefab ra Scene, gán sẵn dữ liệu vào Inspector, nhấn Play và xem progress tự động tăng.
  - *Files:* `CookingTower.cs`.

## Task 3: UI Lò Nướng (Tái sử dụng Banner & InventorySlotUI)
- [ ] Bật Canvas, dùng asset `banner` và `banner_slots` tạo Panel Lò Nướng (`CookingTowerPanel.prefab`).
- [ ] Sắp xếp 3 Slot: Fuel (dưới), Input (trên), Output (phải). Dùng lại Prefab `InventorySlot` (có gắn `InventorySlotUI`).
- [ ] Thêm 2 thanh Progress: Fuel (lửa) và Cook (mũi tên/thanh ngang).
- [ ] Viết `CookingTowerUI.cs` nhận reference từ `CookingTower` để hiển thị data. Đăng ký sự kiện Click từ các `InventorySlotUI` để rút đồ.
  - *Acceptance:* Giao diện hiển thị đúng tài nguyên đồ họa (banner) và cập nhật số liệu. Lấy được đồ từ Output slot.
  - *Verify:* Bấm Play xem UI cập nhật theo Logic.
  - *Files:* `CookingTowerPanel.prefab`, `CookingTowerUI.cs`.

## Task 4: Kết nối Lò Nướng với Inventory (Interaction)
- [ ] Xử lý click phải chuột vào lò (trong khoảng cách): Mở `CookingTowerPanel` và `InventoryPanel`.
- [ ] Khi click vào 1 slot trong Inventory (lúc Lò đang mở):
  - Lấy sự kiện click: Nếu item là Fuel (`fuelTime > 0`) -> Đẩy vào slot Fuel. Nếu item nấu được (`canBeSmelted == true`) -> Đẩy vào slot Input.
- [ ] Khi đóng UI: Đóng cả Lò nướng và Inventory.
  - *Acceptance:* Người chơi click vào thịt/củi trong túi sẽ tự nhảy vào lò. Nấu xong click vào thịt chín sẽ bay lại vào túi.
  - *Verify:* Test toàn bộ luồng trong Play Mode.
  - *Files:* `PlayerInteractionController.cs` (hoặc Controller tương đương), `CookingTowerUI.cs`.
