using UnityEngine;

[System.Serializable]
public class Equipment : Item // Kế thừa từ Item
{
    // Theo UML (#) là protected. Dùng double như UML đã vẽ.
    [SerializeField] protected double maxDurability;
    [SerializeField] protected double currentDurability;

    public void DecreaseDurability(double amount)
    {
        currentDurability -= amount;
        Debug.Log($"{itemName} bị giảm độ bền, hiện tại còn: {currentDurability}");

        if (currentDurability <= 0)
        {
            Debug.Log($"{itemName} đã bị hỏng!");
            // Gọi hàm Delete() từ lớp cha Item
            Delete();
        }
    }
}