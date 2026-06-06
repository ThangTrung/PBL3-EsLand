# Plan: Project Code Cleanup and Simplification

## Overview
Dự án sẽ được "đại tu" qua 6 giai đoạn, đi từ hạ tầng đến ngọn, nhằm loại bỏ các dấu vết của mã nguồn AI rời rạc và thay thế bằng kiến trúc chuyên nghiệp.

---

## Phase 1: Directory & Architectural Alignment
- **Mục tiêu:** Sắp xếp lại file, xóa file rác, đảm bảo thư mục dự án sạch sẽ.
- [ ] Task 1.1: Quét và xóa các file script "mồ côi" (không được gán vào đâu).
- [ ] Task 1.2: Hợp nhất các thư mục `Core`, `Infrastructure` và `Gameplay` nếu có sự chồng chéo.
- [ ] Task 1.3: Review file `.meta` và các asset bị mất reference.

## Phase 2: Core Contracts & Data Simplification
- **Mục tiêu:** Làm sạch các Interface và ScriptableObjects.
- [ ] Task 2.1: Hợp nhất các Interface trùng lặp trong `Core.Contracts`.
- [ ] Task 2.2: Tối ưu hóa các lớp `ItemData`, `BuildingData` (Xóa các biến "có thể dùng sau này" nhưng hiện tại không dùng).
- [ ] Task 2.3: Đơn giản hóa `GameData` cho hệ thống Save.

## Phase 3: Character & AI Refactoring
- **Mục tiêu:** Đơn giản hóa Player và Quái vật.
- [ ] Task 3.1: Refactor `CharacterHealth.cs` thành một component "dumb" chỉ giữ số liệu, đẩy logic xử lý ra Service hoặc Controller.
- [ ] Task 3.2: Hợp nhất logic di chuyển giữa `PlayerMovementController` và `EnemyMovementController` nếu có thể (Shared Base).
- [ ] Task 3.3: Review lại AI State Machine, loại bỏ các State dư thừa hoặc không hoạt động.

## Phase 4: World & Spawning Optimization
- **Mục tiêu:** Làm mượt logic sinh sản và tương tác môi trường.
- [ ] Task 4.1: Tối ưu hóa `EnemySpawnDirector` và `SpawnArea` (Giảm bớt sự phụ thuộc lẫn nhau).
- [ ] Task 4.2: Hợp nhất `ResourceNode`, `TreeResource`, `RockResource` thành một hệ thống linh hoạt hơn, ít code lặp hơn.
- [ ] Task 4.3: Review lại `ElevationAgent` và hệ thống Layer.

## Phase 5: UI & UX Consistency
- **Mục tiêu:** Tách biệt Logic khỏi UI hoàn toàn.
- [ ] Task 5.1: Review `UIManager`, đảm bảo nó chỉ điều phối các Panel, không chứa logic gameplay.
- [ ] Task 5.2: Tối ưu hóa các Slot UI và hệ thống Tooltip.
- [ ] Task 5.3: Hợp nhất các hệ thống Crafting UI và Inventory UI dùng chung component.

## Phase 6: Final Polish & Documentation
- **Mục tiêu:** "Đóng gói" dự án.
- [ ] Task 6.1: Xóa bỏ toàn bộ Log debug thừa thải.
- [ ] Task 6.2: Viết comment chuẩn cho các hàm API quan trọng.
- [ ] Task 6.3: Kiểm tra lần cuối các lỗi Performance (Lag, Memory Leak).

---

## Risks & Mitigations
| Risk | Impact | Mitigation |
|------|--------|------------|
| Mất liên kết (Missing Reference) trong Inspector | Cao | Thực hiện lệnh Save Assets thường xuyên, kiểm tra Reference sau mỗi lần refactor. |
| Phá vỡ hệ thống Save (GameData change) | Trung bình | Tích hợp cơ chế Versioning cho file Save hoặc xóa file save cũ sau khi đổi cấu trúc. |
| Logic game bị thay đổi ngoài ý muốn | Cao | Chạy test manual cho mỗi task hoàn thành. |
