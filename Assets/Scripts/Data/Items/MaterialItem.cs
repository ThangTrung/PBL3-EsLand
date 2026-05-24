using Core.Contracts.Shared;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Inventory/ItemData/Material")]
    public class MaterialItem : ItemData, ICookable
    {
        [Header("Crafting Settings (ICookable)")]
        [SerializeField] private bool canBeSmelted; 
        [SerializeField] private ItemData resultItem;  
        [SerializeField] private float smeltTime = 5f;

        // Implement ICookable
        public bool IsCookable => canBeSmelted;
        public ItemData CookingResult => resultItem;
        public float CookingTime => smeltTime;
    }
}
