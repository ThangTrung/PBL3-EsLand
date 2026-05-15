using System.Collections.Generic;

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

[System.Serializable]
public class GameData
{
    public InventorySaveData inventory;
    // Tương lai ông thêm Máu, Tọa độ, Quái vật vào đây...

    // Constructor khởi tạo data mặc định khi New Game
    public GameData()
    {
        inventory = new InventorySaveData();
    }
}