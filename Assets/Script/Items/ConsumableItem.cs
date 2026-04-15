using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
    public class ConsumableItem : Item
    {
        [Header("Consumable Stats")]
        [SerializeField] private float healthRegen;
        [SerializeField] private float hungerRegen;
        [SerializeField] private float thirstRegen;

        public override bool Use(Entities.Character user)
        {
            if (user is Entities.Player player)
            {
                player.Consume(hungerRegen, thirstRegen, healthRegen);
                return true;
            }
            
            user.Heal(healthRegen);
            return true;
        }
    }
}