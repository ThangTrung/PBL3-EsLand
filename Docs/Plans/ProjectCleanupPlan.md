# Implementation Plan - Project Cleanup & Sync

Dọn dẹp và đồng bộ cấu hình Player và Tài nguyên (Trees & Rocks). 
*Lưu ý: Không can thiệp vào Enemy theo yêu cầu.*

## 1. Thành phần thực hiện (Implementation Components)
- **Player Prefab Sync:** Gán `EquipmentManager` cho `Pawn_black.prefab` và đảm bảo các tham chiếu nội bộ chuẩn xác.
- **Enhanced ResourceSetupUtility:** 
    - Mở rộng logic tìm kiếm: Xử lý cả "Tiny_tree" và "Rock".
    - Chuẩn hóa Layer: Tự động đưa về Layer 12 (`Interactable`).
    - Cấu hình chỉ số: Đá mặc định 10 máu, Cây mặc định 3 máu.
    - Xử lý Animator: Chỉ gán/kiểm tra Animator cho Cây, bỏ qua đối với Đá.

## 2. Thứ tự thực hiện (Implementation Order)
1. **Cập nhật Player Prefab:** Sử dụng MCP để add component vào file `.prefab` gốc.
2. **Nâng cấp ResourceSetupUtility.cs:** 
    - Thêm hằng số `INTERACTABLE_LAYER = 12`.
    - Thêm mảng nhận diện tên (`"Tiny_tree"`, `"Rock"`).
    - Cập nhật hàm `ProcessResource` để phân biệt loại tài nguyên nhằm gán máu phù hợp.
3. **Thực thi Utility:** Chạy script trong Editor để quét và fix toàn bộ Scene hiện tại.

## 3. Rủi ro và Giải pháp (Risks & Mitigation)
- **Rủi ro:** Gán nhầm Layer cho các object không phải tài nguyên nhưng có tên tương tự.
- **Giải pháp:** Kiểm tra sự tồn tại của `Collider2D` hoặc `SpriteRenderer` trước khi xử lý, hoặc chỉ quét các object có chứa chính xác từ khóa trong tên.
- **Rủi ro:** Ghi đè chỉ số máu mà User đã dày công chỉnh sửa thủ công.
- **Giải pháp:** Chỉ gán giá trị mặc định (10 cho Đá) nếu component `ResourceNode` vừa mới được add vào (New component). Nếu đã có sẵn, giữ nguyên giá trị cũ.

## 4. Điểm kiểm chứng (Verification Checkpoints)
- [ ] Prefab `Pawn_black` có component `EquipmentManager`.
- [ ] Chạy "Setup All Resources" báo cáo số lượng object đã xử lý (Green log).
- [ ] Click vào 1 hòn Đá trong Scene: Kiểm tra Layer là 12, có script `ResourceNode` với Max Health = 10.
- [ ] Click vào 1 cái Cây trong Scene: Kiểm tra Layer là 12, có script `TreeResource`.
- [ ] Kiểm tra Console: Không có lỗi biên dịch.
