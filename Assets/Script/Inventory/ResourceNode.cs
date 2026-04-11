using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    // Các thuộc tính theo đúng UML
    [SerializeField] private string nodeName = "Cây sồi cổ thụ";
    [SerializeField] private string nodeType = "Wood";
    [SerializeField] private double health = 30;

    // Danh sách các vật phẩm sẽ rớt ra khi cái cây này bị chặt đứt
    [SerializeField] private List<Item> dropItems = new List<Item>();

    // Test nhanh: Bấm phím E để giả lập việc Player đang chặt cây
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(10); // Mỗi nhát chém mất 10 máu
        }
    }

    public void TakeDamage(double damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Chát! Đang chặt {nodeName}... Máu còn: {health}");

        if (health <= 0)
        {
            // Cây chết -> Gọi hàm rớt đồ
            List<Item> itemsToDrop = DropItem();

            // Tìm túi đồ của Player trong màn chơi và nhét đồ vào
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
            {
                foreach (Item item in itemsToDrop)
                {
                    playerInventory.AddItem(item);
                }
            }

            // Xóa cái cây khỏi màn hình
            Destroy(gameObject);
        }
    }

    // Hàm trả về danh sách vật phẩm rớt ra theo UML
    public List<Item> DropItem()
    {
        Debug.Log($"RẦM! {nodeName} đã bị đốn hạ!");
        return dropItems;
    }
}