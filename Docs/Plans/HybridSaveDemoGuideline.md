# Hướng dẫn Demo & Báo cáo Tính năng Hybrid Save (PBL3-EsLand)

## 1. Giải thích Kỹ thuật (Technical Rationale)

### Tại sao phải nhập IP?
Trong môi trường mạng nội bộ (LAN), địa chỉ IP của máy Server thường thay đổi mỗi khi khởi động lại hoặc đổi mạng Wifi (do giao thức **DHCP**). Việc cho phép nhập IP động qua UI giúp hệ thống linh hoạt, không cần sửa code mỗi khi môi trường mạng thay đổi.

### Cơ chế Hybrid Save hoạt động thế nào?
Hệ thống áp dụng mô hình **"Write-Through Cache"** với Local làm cache và Cloud làm lưu trữ chính:
- **Tính toàn vẹn:** Dữ liệu luôn được lưu vào Local trước để đảm bảo không mất mát nếu mất điện đột ngột.
- **Tính đồng bộ:** Bản sao dữ liệu được đẩy lên SQL Server để người chơi có thể đăng nhập trên máy khác và tiếp tục hành trình.
- **Tính sẵn sàng (Fallback):** Nếu Cloud gặp sự cố, game tự động chuyển sang dùng data Local, đảm bảo trải nghiệm người chơi không bị gián đoạn.

---

## 2. Kịch bản Demo 4 bước

| Bước | Hành động | Mục tiêu trình diễn |
|---|---|---|
| **1. Setup** | Chạy `node server.js` trên máy Server. Chạy `ipconfig` để lấy IP. | Giới thiệu hạ tầng Backend. |
| **2. Login** | Trên máy Client, nhập IP vừa lấy, nhập UserID `Student_01`, bật "Enable Cloud". | Trình diễn cấu hình động (Dynamic Endpoint). |
| **3. Action** | Vào game, nhặt vài món đồ, di chuyển nhân vật, nhấn nút **Save**. | Trình diễn đồng bộ song song (Hybrid Sync). |
| **4. Verify** | Xóa data local trên máy Client (hoặc sang máy khác), login lại đúng UserID. | Chứng minh dữ liệu đã được bảo vệ trên Cloud thành công. |

---

## 3. Từ khóa Báo cáo (Slide Keywords)

- **SOLID - Dependency Injection:** Tiêm cấu hình IP/ID từ UI vào Handler thay vì hardcode.
- **Hybrid Persistence:** Kết hợp sức mạnh của File System (Local) và RDBMS (Cloud).
- **Fallback Mechanism:** Cơ chế dự phòng đảm bảo tính sẵn sàng (Availability).
- **Asynchronous Networking:** Xử lý mạng bất đồng bộ giúp game mượt mà (Non-blocking).
- **Dynamic Endpoint:** Khả năng thích ứng với hạ tầng mạng biến động (DHCP).

---

## 4. Hướng dẫn thiết lập UI trong Unity

1. Tạo một **Panel** mới trong Scene Menu đặt tên là `LoginPanel`.
2. Thêm các Component:
   - `TMP_InputField` cho **Server IP**.
   - `TMP_InputField` cho **User ID**.
   - `Toggle` cho **Cloud Mode**.
   - `TextMeshProUGUI` cho **Status Text**.
3. Kéo Script `LoginUIController.cs` vào `LoginPanel`.
4. Kéo các tham chiếu Component vào các ô tương ứng trong Inspector.
5. Đảm bảo `SaveLoadManager` có mặt trong Scene (thường là trong prefab `GameManager` chạy xuyên scene).
