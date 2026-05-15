using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Inventory/ItemData/Material")]
    public class MaterialItem : ItemData
    {
        [Header("Crafting Settings")]
        public bool canBeSmelted; 
        public ItemData resultItem;  
        public float smeltTime = 5f; 
    }
}
