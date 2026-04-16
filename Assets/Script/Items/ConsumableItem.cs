using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Item/Consumable")]
    public class ConsumableItem : Item
    {
        [Header("Consumable Stats")]
        public int healthRestore;
        public int hungerRestore;
        public int thirstRestore;

        // Ghi đè hàm Use của lớp cha
        // public override void Use()
        // {
        //     base.Use();
        //     // TODO: Gọi script PlayerStats để cộng máu, độ đói...
        //     Debug.Log($"Hồi {healthRestore} Máu và {hungerRestore} Thức ăn!");
        // }
    }
}