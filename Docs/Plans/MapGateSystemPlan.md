# Plan: Map Gate Logic Implementation

## Overview
Tập trung hoàn thiện script điều khiển và quy trình kết nối logic giữa Boss và Cổng.

## Implementation Order
1. **Task 1: Optimize MapGateController Script**
   - Thêm kiểm tra an toàn (Validation) trong `Start()`.
   - Cải thiện hệ thống Log để dễ theo dõi.
   - Đảm bảo việc đăng ký/hủy đăng ký sự kiện chuẩn xác.
2. **Task 2: Integration Testing**
   - Tạo một "Logic Prefab" mẫu (chứa script và một khối Cube làm vật cản).
   - Test thủ công với một Boss giả lập để xác nhận logic mở và logic Save/Load.

## Tasks
- [x] Task 1: Cập nhật và tối ưu `MapGateController.cs`.
  - Acceptance: Script chạy mượt, có cảnh báo nếu thiếu reference, không lỗi leak event.
- [ ] Task 2: Kiểm tra tính tương thích với Save System.
  - Acceptance: Trạng thái cổng được lưu và tải đúng ID.
