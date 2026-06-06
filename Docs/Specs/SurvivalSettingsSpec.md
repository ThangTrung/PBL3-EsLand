# Spec: Centralized Survival Settings (ScriptableObject)

## Objective
Tập trung hóa tất cả các hằng số liên quan đến hệ thống sinh tồn (Máu, Đói, Khát, Thể lực) vào một file duy nhất (`ScriptableObject`). Điều này giúp việc cân bằng game (Game Balance) trở nên cực kỳ dễ dàng mà không cần chỉnh sửa từng script riêng lẻ.

## Mechanics
1. **SurvivalSettings (ScriptableObject):**
    - Chứa các thông số `MaxStats` (Hunger, Thirst, Stamina).
    - Chứa các thông số `DrainRates` (Tốc độ tiêu hao theo thời gian).
    - Chứa các thông số `RestoreAmounts` (Lượng hồi phục khi ăn/uống).
    - Chứa các thông số `Debuffs` (Phạt khi chỉ số thấp).

2. **Integration:**
    - `PlayerSurvivalController`: Đọc `MaxStats` và `DrainRates` từ file settings.
    - `PlayerInteractionController`: Đọc `RestoreAmounts` (vd: `thirstRestoreAmount`) từ file settings.

## Tech Stack
- Unity 2022.3+
- C# (ScriptableObject)

## Project Structure
- `Assets/Scripts/Data/Survival/SurvivalSettings.cs` (New)
- `Assets/Settings/Survival/DefaultSurvivalSettings.asset` (New Asset)
- `Assets/Scripts/Gameplay/Characters/PlayerSurvivalController.cs` (Update)
- `Assets/Scripts/Gameplay/Characters/PlayerInteractionController.cs` (Update)

## Success Criteria
- Thay đổi con số trong file `.asset` sẽ lập tức thay đổi hành vi trong game (vd: đổi tốc độ đói, đổi lượng nước hồi phục).
- Không còn các con số "cứng" (Magic numbers) rải rác trong code logic.
- Dễ dàng tạo thêm các bộ cài đặt khác (Hard Mode, Easy Mode).
