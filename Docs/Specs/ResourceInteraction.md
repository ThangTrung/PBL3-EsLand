# Spec: Tự động di chuyển và tương tác với Tài nguyên (Cây, Đá)

## Objective
Khi người chơi nhấn chuột trái vào một tài nguyên (Cây, Hòn đá) trên bản đồ, nhân vật (Player) đang ở xa sẽ tự động di chuyển đến gần đối tượng đó. Khi nhân vật chạm vào collider của tài nguyên, nhân vật sẽ dừng lại và thực hiện hành động đánh (gọi hàm tương tác). Tài nguyên sau khi bị đánh sẽ nhận sát thương, cập nhật máu và kích hoạt Animator (chuyển trạng thái "Hit") để tạo hiệu ứng rung lên báo hiệu bị trúng đòn.

## Tech Stack
- Unity 6000+ (C#)
- URP (Universal Render Pipeline) / 2D
- Mô hình kiến trúc Component-based kết hợp Interfaces (SOLID)

## Commands
*Dự án Unity sử dụng Editor là chính, tuy nhiên đối với các lệnh test/build CLI nếu có:*
- Mở project: Khởi chạy qua Unity Hub
- Test: Chạy Play Mode trong Unity Editor
- Build: Thực hiện qua menu `File > Build Settings` hoặc `mcp_unityMCP_manage_build`

## Project Structure
Các thư mục liên quan đến tính năng này:
- `Assets/Scripts/Core/Contracts/Shared/` → Chứa các interface như `IInteractable`, `IDamageable`
- `Assets/Scripts/Gameplay/Characters/` → Chứa `PlayerInputController`, `PlayerMovementController`, `PlayerInteractionController`
- `Assets/Scripts/Gameplay/World/` → Chứa `ResourceNode`, xử lý logic nhận sát thương của cây/đá
- `Assets/Prefabs/` → Nơi lưu trữ prefab của Cây, Đá, Nhân vật (Đúng chuẩn thư mục)

## Code Style
Code cần tuân thủ C# naming conventions, tách biệt logic và đảm bảo Clean Code (sử dụng Interface).
Ví dụ:
```csharp
public class ResourceNode : MonoBehaviour, IInteractable, IDamageable
{
    [SerializeField] private float maxHealth = 3f;
    public event Action<float> OnHealthChanged;

    public void Interact(Character interactor)
    {
        // Gọi logic tính toán sát thương từ interactor và truyền vào TakeDamage
    }

    public void TakeDamage(float amount, Character source = null)
    {
        // Trừ máu, gọi event và trigger Animator rung
    }
}
```

## Testing Strategy
- **Play Mode Test:**
  - Kéo prefab Player và prefab Tree/Rock vào Scene.
  - Bấm Play, click chuột vào Tree/Rock.
  - Kiểm tra xem Player có di chuyển tới đúng vị trí Tree/Rock không.
  - Kiểm tra Console log xem sát thương đã được apply chưa.
  - Quan sát Animator của Tree/Rock xem có chuyển state "Hit" không.
- **Inspector Verification:**
  - Đảm bảo Layer của Tree/Rock được set đúng chuẩn (Interactable).
  - Đảm bảo Animator Controller có parameter tên `Hit` (Trigger).
  - Đảm bảo Player có gắn đủ `PlayerInputController`, `PlayerMovementController`, và `PlayerInteractionController`.

## Boundaries
- **Always do:** 
  - Kế thừa từ `IInteractable` và `IDamageable` cho các object có thể tương tác và nhận sát thương.
  - Lưu prefab vào thư mục `Assets/Prefabs/` sau khi cấu hình xong.
  - Kiểm tra null reference trước khi gọi Component (như `Animator`).
- **Ask first:** 
  - Cấu trúc lại toàn bộ Input System hiện tại (nếu đổi qua Input System UI mới).
  - Thay đổi Core Interfaces (`IInteractable`, `IDamageable`).
- **Never do:** 
  - Hardcode logic nhận sát thương trực tiếp vào InputController.
  - Dùng `FindObjectOfType` trong Update.
  - Tự ý lấy assets nằm ngoài `Asset Tiny Swords` và `Tiny Swords old version`.

## Success Criteria
- [ ] Nhấp chuột trái vào Tree/Rock -> Player tự động tìm đường/di chuyển tới Tree/Rock.
- [ ] Khi Player chạm vào Tree/Rock (kiểm tra qua Collider2D.IsTouching) -> Player dừng lại.
- [ ] Player kích hoạt animation chém/đánh (Trigger "interact").
- [ ] Tree/Rock bị trừ máu.
- [ ] Tree/Rock kích hoạt Trigger "Hit" trên Animator (rung lên).
- [ ] (Tuỳ chọn) Nếu máu <= 0 thì Tree/Rock bị phá hủy.

## Open Questions
1. Hiện tại đã có code cho `PlayerInputController`, `PlayerInteractionController`, `PlayerMovementController` và `ResourceNode`, tuy nhiên có thể phần setup Prefab, Collider, Animator và Layer trên Scene chưa hoàn thiện. Bạn có muốn tôi tiến hành check các Prefab của Cây/Đá và Player để đảm bảo chúng đã được setup Animator Controller và Layer chuẩn chưa?
2. Có cần xử lý tìm đường (Pathfinding bằng NavMesh) khi Player di chuyển tới cây/đá không, hay chỉ cần di chuyển theo đường thẳng (Vector thẳng) như hiện tại?
