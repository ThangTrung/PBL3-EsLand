using Core.Contracts.Shared;
using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/ItemData/Consumable")]
    public class ConsumableItem : ItemData, IItemUsable
    {
        [Header("Consumable Stats")]
        public float healthRestore;
        public float hungerRestore;

        public bool Use(Character user)
        {
            if (user == null) return false;
            
            var used = false;
            
            if (healthRestore > 0f && user.Health != null)
            {
                user.Health.Heal(healthRestore);
                used = true;
            }
            
            var survival = user.GetComponentInChildren<PlayerSurvivalController>();
            if (survival != null)
            {
                if (hungerRestore > 0f)
                {
                    survival.AddHunger(hungerRestore);
                    used = true;
                }
            }

            if (used)
            {
            }
            
            return used;
        }
    }
}
