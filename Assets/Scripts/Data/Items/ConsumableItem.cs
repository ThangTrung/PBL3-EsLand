using Core.Contracts.Shared;
using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/ItemData/Consumable")]
    public class ConsumableItem : ItemData, IItemUsable, ICookable
    {
        [Header("Consumable Stats")]
        public float healthRestore;
        public float hungerRestore;
        public float thirstRestore;

        [Header("Cooking Settings (ICookable)")]
        [SerializeField] private bool canBeCooked;
        [SerializeField] private ItemData cookedResult;
        [SerializeField] private float cookingTime = 10f;

        // Implement ICookable
        public bool IsCookable => canBeCooked;
        public ItemData CookingResult => cookedResult;
        public float CookingTime => cookingTime;

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
                
                if (thirstRestore > 0f)
                {
                    survival.AddThirst(thirstRestore);
                    used = true;
                }
            }

            if (used)
            {
                Debug.Log($"Đã sử dụng {ItemName}. Hồi {healthRestore} Máu, {hungerRestore} Thức ăn, {thirstRestore} Nước uống cho {user.CharacterName}!");
            }
            
            return used;
        }
    }
}
