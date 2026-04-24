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

        public override bool Use(Entities.Character user)
        {
            base.Use(user);
            Debug.Log($"Hồi {healthRestore} Máu và {hungerRestore} Thức ăn cho {user.CharacterName}!");
            return true;
        }
    }
}