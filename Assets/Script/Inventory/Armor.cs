using UnityEngine;

[System.Serializable]
public class Armor : Equipment // Kế thừa từ Equipment
{
    [SerializeField] private double defensePower;
    [SerializeField] private string slotType; // Ví dụ: "Đầu", "Thân", "Chân"

    public override bool Use()
    {
        Debug.Log($"Đã mặc {itemName} vào vị trí [{slotType}] - Tăng {defensePower} điểm phòng thủ.");
        return true;
    }
}