using UnityEngine;

// Bắt buộc phải có [System.Serializable] để Unity có thể hiển thị class này trên bảng Inspector
[System.Serializable]
public class Item
{
    // Dùng protected (# trong UML) để các lớp con (Equipment, Consumable) có thể xài chung
    // [SerializeField] giúp biến dù bị khóa vẫn hiện lên Unity để bạn dễ dàng nhập data test
    [SerializeField] protected string itemName;
    [SerializeField] protected string description;
    [SerializeField] protected int maxStack;

    // Constructor (Hàm khởi tạo) để sau này dễ dàng tạo item mới bằng code
    public Item() { }
    public string ItemName { get { return itemName; } }
    public Item(string name, string desc, int stack)
    {
        this.itemName = name;
        this.description = desc;
        this.maxStack = stack;
    }

    // Hàm ảo (virtual) để các lớp con có thể ghi đè (override) logic sử dụng riêng
    public virtual bool Use()
    {
        Debug.Log($"Đang sử dụng vật phẩm cơ bản: {itemName}");
        return true;
    }

    public virtual bool Delete()
    {
        Debug.Log($"Đã vứt vật phẩm: {itemName}");
        return true;
    }
}