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

    // 🔥 ĐÃ ĐỒNG BỘ: Khớp hoàn toàn với logic của script ResourceNode.cs
    [System.Serializable]
    public class ResourceNodeSaveData
    {
        public string nodeID;      // GUID định danh duy nhất của Cây/Đá
        public float currentHP;    // Máu hiện tại lúc đang bị chặt dở
        public bool isStump;       // true = đã thành gốc cây, false = cây nguyên vẹn
    }

    [System.Serializable]
    public class GameData
    {
        [Header("Inventory Systems")]
        public List<InventorySaveData> inventories = new List<InventorySaveData>();

        [Header("World State")]
        // 🔥 ĐÃ ĐỔI TÊN: Thành resourceNodes cho giống logic trong code gọi data
        public List<ResourceNodeSaveData> resourceNodes = new List<ResourceNodeSaveData>();
        
        public List<DroppedItemSaveData> droppedItems = new List<DroppedItemSaveData>();
        
        // Sổ đen: Lưu danh sách ID các viên đá/cành cây nhặt dưới đất đã bị xóa sổ
        public List<string> destroyedEntityIDs = new List<string>(); 

        [Header("Player Stats")]
        public float playerHealth;
        public Vector3 playerPosition;

        // Constructor: Trong C# hiện đại, vì ông đã khởi tạo = new List<>() ở trên rồi, 
        // nên trong này chỉ cần khởi tạo các thông số value type là đủ, tránh bị chạy 2 lần.
        public GameData()
        {
            playerHealth = 100f;
            playerPosition = Vector3.zero;
        }
    }
}