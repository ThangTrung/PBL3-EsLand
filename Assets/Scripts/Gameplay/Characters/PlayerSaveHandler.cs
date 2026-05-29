using UnityEngine;
using Infrastructure.SaveSystem.Core; 
using Infrastructure.SaveSystem.Data;
using Core.Contracts.Shared;

namespace Gameplay.Characters
{
    public class PlayerSaveHandler : MonoBehaviour, ISaveable
    {
        private IRespawnable _respawnable;

        private void Awake()
        {
            _respawnable = GetComponent<IRespawnable>();
        }

        // Khi Load game: Kéo nhân vật về tọa độ đã lưu và thiết lập điểm hồi sinh
        public void LoadData(GameData data)
        {
            // Tránh trường hợp game mới tinh ép nhân vật về (0,0,0) nếu map ông không bắt đầu ở đó.
            if (data.playerPosition != Vector3.zero)
                transform.position = data.playerPosition;

            if (_respawnable != null && data.respawnPoint != Vector3.zero)
                _respawnable.RespawnPoint = data.respawnPoint;
        }

        // Khi Save game: Ghi tọa độ hiện tại và điểm hồi sinh vào sổ
        public void SaveData(GameData data)
        {
            data.playerPosition = transform.position;
            
            if (_respawnable != null)
                data.respawnPoint = _respawnable.RespawnPoint;
        }
    }
}