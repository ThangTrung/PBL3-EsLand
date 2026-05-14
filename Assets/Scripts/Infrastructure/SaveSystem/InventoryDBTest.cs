using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Gameplay.Inventory; 
using Data.Items;         

[System.Serializable]
public class ItemSaveData
{
    public string itemID;
    public int quantity;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> savedItems = new List<ItemSaveData>();
}

public class InventoryDBTest : MonoBehaviour
{
    private string savePath;

    [Header("Kéo cục Player chứa InventoryController vào đây")]
    public InventoryController inventoryController;

    void Start()
    {
        savePath = Application.persistentDataPath + "/inventoryLocalDB.json";

        // Đợi 1 frame cho Inventory khởi tạo xong rồi mới Load đồ
        Invoke(nameof(LoadFromDatabase), 0.1f);
    }

    void Update()
    {
        // Bấm phím M để test lưu game thực tế
        if (Input.GetKeyDown(KeyCode.M))
        {
            SaveToDatabase();
        }
    }

    public void SaveToDatabase()
    {
        // 1. Nếu quên kéo refernce thì báo lỗi
        if (inventoryController == null)
        {
            Debug.LogError("Bạn chưa kéo InventoryController vào!");
            return;
        }

        InventorySaveData data = new InventorySaveData();

        // 2. Quét qua cái biến Slots mà ông vừa tìm ra đó!
        foreach (var slot in inventoryController.Slots)
        {
            // Chỉ lưu những ô có đồ (dùng hàm IsEmpty của ông)
            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                data.savedItems.Add(new ItemSaveData
                {
                    // Lấy đúng tên file ScriptableObject làm ID
                    itemID = slot.Item.name,
                    quantity = slot.Amount
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("<color=cyan>ĐÃ LƯU THỰC TẾ TÚI ĐỒ:</color>\n" + json);
    }

    public void LoadFromDatabase()
    {
        if (inventoryController == null) return;

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

            // Xóa sạch túi đồ hiện tại bằng hàm Clear() có sẵn trong code của ông
            inventoryController.Clear();
            Item[] allGameItems = Resources.LoadAll<Item>("Data/Items");
            foreach (var savedItem in data.savedItems)
            {
                // 2. Tìm kiếm: Soi xem file nào có cái tên (name) trùng khớp với ID đã lưu
                Item itemAsset = System.Array.Find(allGameItems, i => i.name == savedItem.itemID);

                if (itemAsset != null)
                {
                    // Dùng hàm AddItem có sẵn của ông để nhét đồ vào lại
                    inventoryController.AddItem(itemAsset, savedItem.quantity);
                    Debug.Log($"-> Phục hồi thành công: {savedItem.quantity}x {savedItem.itemID}");
                }
                else
                {
                    Debug.LogWarning($"Không tìm thấy file tĩnh của: {savedItem.itemID}. Hãy check lại folder Resources!");
                }
            }
        }
    }
}