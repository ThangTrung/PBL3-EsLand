using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Inventory/Item/Material")]
    public class MaterialItem : Item
    {
        [Header("Crafting Settings")]
        public bool canBeSmelted; 
        public Item resultItem;  
        public float smeltTime = 5f; 
    }
}