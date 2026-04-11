using UnityEngine;

[System.Serializable]
public class Tool : Equipment // Kế thừa từ Equipment
{
    // Dấu (-) trong UML tức là private
    [SerializeField] private double combatDamage;
    [SerializeField] private double gatheringPower;
    [SerializeField] private string toolType;

    // Tính đa hình trong OOAD: Ghi đè hàm Use() của lớp Item cha
    public override bool Use()
    {
        Debug.Log($"Đang vung {itemName} (Loại: {toolType}) với sức thu thập {gatheringPower}!");
        // Mỗi lần dùng thì giảm 1 độ bền
        DecreaseDurability(1.0);
        return true;
    }
}