# Spec: Project Cleanup & Configuration Sync

## Objective
Dọn dẹp và đồng bộ hóa toàn diện giữa Code và Unity Editor để đảm bảo hệ thống vận hành ổn định. Trọng tâm là sửa lỗi kế thừa trong code, hoàn thiện cấu hình Prefab cho toàn bộ tài nguyên (Cây & Đá), và chuẩn hóa hệ thống Layer tương tác.

## Tech Stack
- Unity 2022.3.62f3 (C#)
- Unity Editor Scripting (Custom Utilities)
- Hệ thống Component-based (SOLID)

## Commands
- **Run Cleanup Utility:** `MenuItem > PBL3 > Resource > Setup All Resources` (Sẽ nâng cấp từ script cũ).
- **Check Compilation:** `mcp_unityMCP_read_console` (Đảm bảo không còn lỗi CS0114).
- **Verify Prefabs:** `mcp_unityMCP_manage_asset` (Kiểm tra sự tồn tại của components).

## Project Structure
- `Assets/Scripts/Gameplay/Characters/Enemy.cs` → Cần sửa logic `Awake`.
- `Assets/Scripts/Editor/ResourceSetupUtility.cs` → Nâng cấp để xử lý cả Đá (Rock) và gán Layer.
- `Assets/Prefabs/` → Đối tượng kiểm tra (Player, Tree, Rock).

## Code Style
- Sử dụng `protected override` cho các hàm khởi tạo trong class kế thừa.
- Editor Script phải sử dụng hệ thống `Undo` và `EditorUtility.SetDirty`.
- Tránh hardcode string; ưu tiên sử dụng hằng số cho Layer Name nếu có thể.

## Testing Strategy
1. **Code Audit:** Kiểm tra file `Enemy.cs` qua MCP để xác nhận đã gọi `base.Awake()`.
2. **Utility Test:** 
   - Chạy script Setup mới.
   - Kiểm tra Prefab `Rock_1`: Phải có `ResourceNode`, `ResourceVisualEffects`, và Layer chuyển thành 12.
3. **Hierarchy Check:** Đảm bảo `Pawn_black` có `EquipmentManager`.

## Boundaries
- **Always do:** Sử dụng `Undo.AddComponent` để User có thể rollback.
- **Ask first:** Nếu muốn thay đổi chỉ số máu mặc định của tài nguyên.
- **Never do:** Sửa trực tiếp file `.prefab` bằng text editor; luôn dùng MCP Tool hoặc Editor Script.

## Success Criteria
- [ ] `Enemy.cs` không còn cảnh báo ẩn member (`CS0114`).
- [ ] Prefab `Pawn_black` (Player) chứa component `EquipmentManager`.
- [ ] Toàn bộ Cây và Đá trong Scene/Prefabs đều thuộc Layer 12 (`Interactable`).
- [ ] Toàn bộ Đá (`Rock`) có đủ logic nhận sát thương và hiệu ứng rung.
- [ ] Script `ResourceSetupUtility` xử lý được cả Cây và Đá chỉ bằng 1 click.

## Open Questions
1. Bạn muốn thiết lập lượng máu (Max Health) mặc định cho Đá là bao nhiêu? (Ví dụ: Cây đang là 3, Đá có thể là 5).
2. Có cần tự động gán một `AnimatorController` mặc định cho các hòn Đá không? (Vì hiện tại chúng đang thiếu cả Animator).
