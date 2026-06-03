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
        [Header("Inventory Systems")]
        public List<InventorySaveData> inventories = new List<InventorySaveData>();

        // 🔥 MỚI THÊM: Sổ ghi chép đồ đang mặc trên người
        public List<EquippedItemSaveData> equippedItems = new List<EquippedItemSaveData>();

        [Header("World State")]
        public List<ResourceNodeSaveData> resourceNodes = new List<ResourceNodeSaveData>();
        public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
        public List<string> openedGates = new List<string>();
        
        public List<string> destroyedEntityIDs = new List<string>(); 
        
        [Header("Enemy States")]
        public List<EnemySaveData> activeEnemies = new List<EnemySaveData>();

        [Header("Player Stats")]
        public float playerHealth;
        public Vector3 playerPosition;
        public Vector3 respawnPoint; // Điểm hồi sinh khi chết

        public GameData()
        {
            playerHealth = 100f;
            playerPosition = Vector3.zero;
            respawnPoint = Vector3.zero;
        }
    }
}