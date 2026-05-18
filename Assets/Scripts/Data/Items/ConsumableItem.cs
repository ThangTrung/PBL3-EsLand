using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/ItemData/Consumable")]
    public class ConsumableItem : ItemData
    {
        [Header("Consumable Stats")]
        public int healthRestore;
        public int hungerRestore;
        public int thirstRestore;

        public override bool Use(Character user)
        {
            base.Use(user);
            Debug.Log($"Hồi {healthRestore} Máu và {hungerRestore} Thức ăn cho {user.CharacterName}!");
            return true;
        }
    }
}
