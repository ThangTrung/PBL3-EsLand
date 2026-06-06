# Spec: Project Code Review and Cleanup

## Objective
Làm sạch, đơn giản hóa và tối ưu hóa toàn bộ mã nguồn của dự án PBL3-EsLand. Chuyển đổi từ mã nguồn được sinh ra bởi AI (có thể rời rạc và dư thừa) thành một hệ thống đồng nhất, tuân thủ các tiêu chuẩn kỹ thuật phần mềm cao cấp, dễ bảo trì và mở rộng.

## Tiêu chuẩn Làm sạch (Cleanup Standards)

### 1. Naming Conventions (Quy tắc đặt tên)
- **Class/Struct:** PascalCase (vd: `PlayerController`).
- **Interface:** Bắt đầu bằng chữ `I` (vd: `ISaveable`).
- **Private Field:** CamelCase với dấu gạch dưới (vd: `_health`).
- **Public Property/Method:** PascalCase (vd: `CurrentHealth`).
- **Local Variable:** CamelCase (vd: `distanceSqr`).

### 2. Architectural Boundaries (Ranh giới Kiến trúc)
- **Logic vs View:** Tuyệt đối không để logic tính toán (VD: tính damage) nằm trong các script UI.
- **Service Pattern:** Sử dụng các Service tập trung (VD: `InventoryService`, `SaveLoadManager`) thay vì để các đối tượng tự quản lý logic phức tạp.
- **Contract-based:** Ưu tiên giao tiếp giữa các module qua Interface thay vì tham chiếu trực tiếp class cụ thể.

### 3. Simplification Rules (Nguyên tắc Đơn giản hóa)
- **DRY (Don't Repeat Yourself):** Hợp nhất các hàm hoặc class thực hiện chức năng tương tự.
- **KISS (Keep It Simple, Stupid):** Loại bỏ các Design Pattern không cần thiết (Over-engineering). Nếu một biến đơn giản có thể giải quyết vấn đề, không dùng hệ thống Event phức tạp.
- **Dead Code Removal:** Xóa bỏ toàn bộ các biến, hàm, và file không còn sử dụng.
- **Comment Policy:** Sử dụng comment tiếng Việt súc tích cho logic phức tạp. Xóa các comment AI thừa thải (VD: "This method updates the health").

### 4. Unity Optimization (Tối ưu hóa Unity)
- **GetComponent:** Hạn chế gọi trong `Update`. Sử dụng caching ở `Awake/Start`.
- **String References:** Sử dụng `Animator.StringToHash` và `Shader.PropertyToID`.
- **Object Pooling:** Đảm bảo toàn bộ các vật thể sinh/hủy nhiều (quái, loot, vfx) đều đi qua `ObjectPoolManager`.

## Success Criteria
- Mã nguồn dễ đọc, không cần giải thích vẫn hiểu được luồng đi.
- Số lượng file script giảm xuống nhưng chức năng không đổi.
- Hiệu năng (FPS) ổn định, không có Spike lag do logic Update dư thừa.
- Không còn lỗi Warning hoặc Error trên Console.
