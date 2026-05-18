using System.Collections.Generic;
using UnityEngine;

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
        public string resourceID;  // ID duy nhất của node tài nguyên
        public bool isDestroyed;
        public float currentHealth;
    }

    [System.Serializable]
    public class GameData
    {
        [Header("Inventory Systems")]
        public List<InventorySaveData> inventories = new List<InventorySaveData>();

        [Header("World State")]
        public List<ResourceNodeSaveData> resources = new List<ResourceNodeSaveData>();
        public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();

        [Header("Player Stats")]
        public float playerHealth;
        public Vector3 playerPosition;

        public GameData()
        {
            inventories = new List<InventorySaveData>();
            resources = new List<ResourceNodeSaveData>();
            droppedItems = new List<DroppedItemSaveData>();
            playerHealth = 100f;
            playerPosition = Vector3.zero;
        }
    }
}
