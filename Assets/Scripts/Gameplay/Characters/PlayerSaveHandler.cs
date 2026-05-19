using UnityEngine;
using Infrastructure.SaveSystem.Core; 
using Infrastructure.SaveSystem.Data; 

namespace Gameplay.Characters
{
    public class PlayerSaveHandler : MonoBehaviour, ISaveable
    {
        // Khi Load game: Kéo nhân vật về tọa độ đã lưu
        public void LoadData(GameData data)
        {
            // Tránh trường hợp game mới tinh ép nhân vật về (0,0,0) nếu map ông không bắt đầu ở đó.
            // Nếu data.playerPosition bằng Vector3.zero (mặc định), ông có thể thêm điều kiện bỏ qua nếu muốn.
            // Nhưng hiện tại cứ gán thẳng để test trước:
            transform.position = data.playerPosition;
        }

        // Khi Save game: Ghi tọa độ hiện tại vào sổ
        public void SaveData(GameData data)
        {
            data.playerPosition = transform.position;
        }
    }
}