using UnityEngine;
using System.Collections.Generic;
using Gameplay.Inventory;
using Data.Items;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Handlers
{
    [RequireComponent(typeof(InventoryController))]
    public class InventorySaveHandler : MonoBehaviour, ISaveable
    {
        [SerializeField] private string inventoryID = "PlayerMain"; // Mặc định là Player
        private InventoryController inventory;

        private void Awake()
        {
            inventory = GetComponent<InventoryController>();
        }

        private void Start()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged += TriggerSave;
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= TriggerSave;
            }
        }

        private void TriggerSave()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame();
            }
        }

        public void LoadData(GameData data)
        {
            if (inventory == null || data == null) return;

            // Tìm đúng Inventory theo ID trong GameData
            InventorySaveData invData = data.inventories.Find(i => i.inventoryID == inventoryID);
            if (invData == null) return;

            inventory.Clear();
            ItemData[] allItems = Resources.LoadAll<ItemData>("Data/Items");

            foreach (var savedItem in invData.slots)
            {
                ItemData itemAsset = System.Array.Find(allItems, i => i.name == savedItem.itemID);
                if (itemAsset != null)
                {
                    // Giả định AddItem có thể nhận durability (cần check InventoryController)
                    // Ở bản demo này ta load item và số lượng trước
                    inventory.AddItem(itemAsset, savedItem.quantity);
                    
                    // Nếu slot hỗ trợ độ bền, ta sẽ set ở đây (cần mở rộng InventorySlot)
                    var slot = inventory.GetSlotAt(savedItem.slotIndex);
                    if (slot != null && slot is InventorySlot concreteSlot)
                    {
                        concreteSlot.SetData(itemAsset, savedItem.quantity, savedItem.currentDurability);
                    }
                }
            }
        }

        public void SaveData(GameData data)
        {
            if (inventory == null || data == null) return;

            // Xóa data cũ của inventory này nếu có
            data.inventories.RemoveAll(i => i.inventoryID == inventoryID);

            InventorySaveData invData = new InventorySaveData();
            invData.inventoryID = inventoryID;

            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var slot = inventory.Slots[i];
                if (slot != null && !slot.IsEmpty && slot.ItemData != null)
                {
                    invData.slots.Add(new ItemSlotSaveData
                    {
                        itemID = slot.ItemData.name,
                        quantity = slot.Amount,
                        slotIndex = i,
                        currentDurability = (int)slot.CurrentDurability
                    });
                }
            }

            data.inventories.Add(invData);
        }
    }
}
