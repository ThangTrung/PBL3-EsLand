using System.Collections.Generic;
using UnityEngine;
using Data.Equipment; // Bổ sung thư viện này để nhận diện EquipSlot

namespace Infrastructure.SaveSystem.Data
{
    [System.Serializable]
    public class ItemSlotSaveData
    {
        public string itemID;      // Tên của ScriptableObject ItemData
        public int quantity;
        public int slotIndex;      // Vị trí slot trong inventory
        public int currentDurability; // Độ bền hiện tại nếu có
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public string inventoryID; // Định danh túi đồ (vd: "PlayerMain", "Chest_01")
        public List<ItemSlotSaveData> slots = new List<ItemSlotSaveData>();
    }

    [System.Serializable]
    public class DroppedItemSaveData
    {
        public string itemID;
        public Vector3 position;
        public int quantity;
        public string uniqueID;    // ID duy nhất để phân biệt các item rơi
    }

    [System.Serializable]
    public class ResourceNodeSaveData
    {
        public string nodeID;      // GUID định danh duy nhất của Cây/Đá
        public float currentHP;    // Máu hiện tại lúc đang bị chặt dở
        public bool isStump;       // true = đã thành gốc cây, false = cây nguyên vẹn
    }

    // 🔥 MỚI THÊM: Cấu trúc để lưu lại món đồ nào đang gắn ở khe (slot) nào
    [System.Serializable]
    public class EquippedItemSaveData
    {
        public EquipSlot slot;
        public string itemID; // Lưu tên file của Item (vd: "Axe", "Defense")
    }

    [System.Serializable]
    public class EnemySaveData
    {
        public string enemyID;       // Dành cho quái tĩnh (GUID)
        public string configID;      // Tên file ScriptableObject Config (để fallback)
        public float currentHP;      // Máu hiện tại lúc thoát game
        public Vector3 position;     // Vị trí đang đứng
        public bool isStaticBoss;    // Đánh dấu đây là quái tĩnh
    }

    [System.Serializable]
    public class GameData
    {
        // Sử dụng backup fields và properties để đảm bảo danh sách không bao giờ null (Lazy Initialization)
        [SerializeField] private List<InventorySaveData> _inventories = new List<InventorySaveData>();
        public List<InventorySaveData> inventories { get => _inventories ??= new List<InventorySaveData>(); set => _inventories = value; }

        [SerializeField] private List<EquippedItemSaveData> _equippedItems = new List<EquippedItemSaveData>();
        public List<EquippedItemSaveData> equippedItems { get => _equippedItems ??= new List<EquippedItemSaveData>(); set => _equippedItems = value; }

        [SerializeField] private List<ResourceNodeSaveData> _resourceNodes = new List<ResourceNodeSaveData>();
        public List<ResourceNodeSaveData> resourceNodes { get => _resourceNodes ??= new List<ResourceNodeSaveData>(); set => _resourceNodes = value; }

        [SerializeField] private List<DroppedItemSaveData> _droppedItems = new List<DroppedItemSaveData>();
        public List<DroppedItemSaveData> droppedItems { get => _droppedItems ??= new List<DroppedItemSaveData>(); set => _droppedItems = value; }

        [SerializeField] private List<string> _openedGates = new List<string>();
        public List<string> openedGates { get => _openedGates ??= new List<string>(); set => _openedGates = value; }
        
        [SerializeField] private List<string> _destroyedEntityIDs = new List<string>(); 
        public List<string> destroyedEntityIDs { get => _destroyedEntityIDs ??= new List<string>(); set => _destroyedEntityIDs = value; }
        
        [SerializeField] private List<EnemySaveData> _activeEnemies = new List<EnemySaveData>();
        public List<EnemySaveData> activeEnemies { get => _activeEnemies ??= new List<EnemySaveData>(); set => _activeEnemies = value; }

        [Header("Player Stats")]
        public float playerHealth;
        public Vector3 playerPosition;
        public Vector3 respawnPoint; // Điểm hồi sinh khi chết

        public GameData()
        {
            // Khởi tạo mặc định
            playerHealth = 100f;
            playerPosition = Vector3.zero;
            respawnPoint = Vector3.zero;
        }
    }
}