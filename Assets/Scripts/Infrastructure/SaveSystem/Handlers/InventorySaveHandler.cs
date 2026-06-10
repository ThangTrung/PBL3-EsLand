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
            if (invData == null) 
            {
                Debug.Log($"[InventorySave] No data found for inventoryID: {inventoryID}");
                return;
            }

            Debug.Log($"[InventorySave] Loading {invData.slots.Count} items into inventory: {inventoryID}");
            
            // 🔥 QUAN TRỌNG: Tạm thời ngắt sự kiện để không bị Save ngược lên Cloud trong khi đang Load
            inventory.OnInventoryChanged -= TriggerSave;
            
            inventory.Clear();

            foreach (var savedItem in invData.slots)
            {
                ItemData itemAsset = ItemDatabase.Instance.GetItemByID(savedItem.itemID);
                if (itemAsset != null)
                {
                    inventory.AddItem(itemAsset, savedItem.quantity);
                    
                    var slot = inventory.GetSlotAt(savedItem.slotIndex);
                    if (slot != null && slot is InventorySlot concreteSlot)
                    {
                        concreteSlot.SetData(itemAsset, savedItem.quantity, savedItem.currentDurability);
                    }
                }
                else
                {
                    Debug.LogWarning($"[InventorySave] ItemID not found in database: {savedItem.itemID}");
                }
            }
            
            // 🔥 Đăng ký lại sự kiện sau khi xong
            inventory.OnInventoryChanged += TriggerSave;
            inventory.NotifyChanged();
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
                        itemID = slot.ItemData.ID, // 🔥 ĐÃ SỬA: Dùng ID (GUID) thay vì .name
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
