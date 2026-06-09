using UnityEngine;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;
using Gameplay.AI;
using Gameplay.Characters;

namespace Gameplay.SaveSystem
{
    /// <summary>
    /// Gắn vào Prefab Enemy để liên kết vòng đời của nó với hệ thống Save/Load.
    /// Tự động cô lập rác dữ liệu: Chỉ ghi file nếu là Static/Boss.
    /// Đảm bảo luôn despawn nếu đã chết trong quá khứ.
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    [RequireComponent(typeof(SaveableEntity))]
    public class EnemySaveHandler : MonoBehaviour, ISaveable
    {
        [Tooltip("Nếu tích: Máu và Tọa độ sẽ được lưu lại.\nNếu không: Bỏ qua (Chỉ lưu nếu đã chết).")]
        [SerializeField] private bool isStaticBoss = false;

        private EnemyBase _enemyBase;
        private SaveableEntity _saveableEntity;
        private CharacterHealth _health;

        private void Awake()
        {
            _enemyBase = GetComponent<EnemyBase>();
            _saveableEntity = GetComponent<SaveableEntity>();
            _health = GetComponent<CharacterHealth>();
        }

        public void SaveData(GameData data)
        {
            // 1. Nếu Enemy đã chết -> Lưu GUID vào "Sổ sinh tử" để Load game nó không hồi sinh
            if (_health != null && _health.IsDead)
            {
                if (!string.IsNullOrEmpty(_saveableEntity.Id) && !data.destroyedEntityIDs.Contains(_saveableEntity.Id))
                {
                    data.destroyedEntityIDs.Add(_saveableEntity.Id);
                }
                return;
            }

            // 2. Nếu Enemy còn sống VÀ là Quái tĩnh (Boss) -> Lưu thông số chiến đấu dở dang
            if (isStaticBoss && !string.IsNullOrEmpty(_saveableEntity.Id))
            {
                // Dọn dẹp bản ghi cũ nếu có để tránh trùng lặp
                data.activeEnemies.RemoveAll(e => e.enemyID == _saveableEntity.Id);

                // Thử lấy tên file Config (dùng để sinh đúng loại quái từ Pool nếu cần)
                string configName = _enemyBase.Config != null ? (_enemyBase.Config as UnityEngine.Object)?.name : "";

                EnemySaveData enemyData = new EnemySaveData
                {
                    enemyID = _saveableEntity.Id,
                    configID = configName,
                    currentHP = _health != null ? _health.CurrentHealth : 100f,
                    position = transform.position,
                    isStaticBoss = true
                };

                data.activeEnemies.Add(enemyData);
            }
        }

        public void LoadData(GameData data)
        {
            if (string.IsNullOrEmpty(_saveableEntity.Id)) return;

            // 1. Kiểm tra Sổ sinh tử: Nếu đã chết trước đây -> Xóa sổ luôn lập tức
            if (data.destroyedEntityIDs.Contains(_saveableEntity.Id))
            {
                // Tùy kiến trúc, có thể Return To Pool hoặc SetActive(false)
                // Vì load xong nó chưa nằm trong Pool tracker, nên tắt đi là an toàn nhất
                gameObject.SetActive(false);
                return;
            }

            // 2. Khôi phục thông số: Nếu là Quái tĩnh và có trong file Save
            if (isStaticBoss)
            {
                var savedData = data.activeEnemies.Find(e => e.enemyID == _saveableEntity.Id);
                if (savedData != null)
                {
                    // Tái thiết lập vị trí
                    transform.position = savedData.position;
                    // Phục hồi máu
                    if (_health != null)
                    {
                        _health.InternalSetHealth(savedData.currentHP);
                    }
                }
            }
        }
    }
}