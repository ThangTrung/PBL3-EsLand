using UnityEngine;
using Gameplay.Inventory;
using Data.Items;

[RequireComponent(typeof(InventoryController))]
public class InventorySaveHandler : MonoBehaviour, ISaveable
{
    private InventoryController inventory;

    // 1. Dùng Awake để khởi tạo các tham chiếu nội bộ
    private void Awake()
    {
        inventory = GetComponent<InventoryController>();
    }

    // 2. Dùng Start để kết nối (Subscribe) sự kiện với các Script khác
    private void Start()
    {
        if (inventory != null)
        {
            // Lúc này chắc chắn InventoryController đã Awake xong, không sợ bị Null nữa
            inventory.OnInventoryChanged += TriggerSave;
        }
    }

    // 3. Tốt nhất là nên hủy đăng ký khi đối tượng bị xóa để tránh lỗi bộ nhớ
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= TriggerSave;
        }
    }

    private void TriggerSave()
    {
        // Kiểm tra Instance để chắc chắn SaveLoadManager đã tồn tại trên Scene
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }

    public void LoadData(GameData data)
    {
        if (inventory == null) return;

        inventory.Clear();
        ItemData[] allItems = Resources.LoadAll<ItemData>("Data/Items");

        foreach (var savedItem in data.inventory.savedItems)
        {
            ItemData itemAsset = System.Array.Find(allItems, i => i.name == savedItem.itemID);
            if (itemAsset != null)
            {
                inventory.AddItem(itemAsset, savedItem.quantity);
            }
        }
    }

    public void SaveData(GameData data)
    {
        if (inventory == null || data == null) return;

        data.inventory.savedItems.Clear();

        foreach (var slot in inventory.Slots)
        {
            if (slot != null && !slot.IsEmpty && slot.ItemData != null)
            {
                data.inventory.savedItems.Add(new ItemSaveData
                {
                    itemID = slot.ItemData.name,
                    quantity = slot.Amount
                });
            }
        }
    }
}