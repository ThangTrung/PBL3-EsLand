using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Các thuộc tính cơ bản theo UML
    public int Capacity = 20;
    public int CurrentLoad = 0;

    // Khởi tạo danh sách chứa vật phẩm (dùng Item làm kiểu dữ liệu chung)
    public List<Item> itemList = new List<Item>();

    // Hàm thêm vật phẩm (Bắt buộc phải có để test nhặt đồ)
    public bool AddItem(Item newItem)
    {
        if (CurrentLoad < Capacity)
        {
            itemList.Add(newItem);
            CurrentLoad++;
            Debug.Log("Đã nhặt vật phẩm: " + newItem.ItemName);
            return true;
        }
        else
        {
            Debug.Log("Túi đồ đã đầy, không thể nhặt thêm!");
            return false;
        }
    }

    // Hàm sử dụng vật phẩm theo UML
    public bool UseItem(Item item)
    {
        if (itemList.Contains(item))
        {
            return item.Use(); // Gọi hàm Use() ảo (virtual) từ lớp Item
        }
        return false;
    }

    // Hàm vứt/xóa vật phẩm theo UML
    public bool RemoveItem(Item item)
    {
        if (itemList.Contains(item))
        {
            itemList.Remove(item);
            CurrentLoad--;
            Debug.Log("Đã vứt vật phẩm.");
            return true;
        }
        return false;
    }
}