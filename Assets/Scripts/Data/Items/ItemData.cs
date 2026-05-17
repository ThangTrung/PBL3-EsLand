using Core.Contracts.Shared;
using Gameplay.Characters;
using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        [Header("Item Basic Info")]
        [SerializeField] protected string itemName = "";
        [SerializeField] [TextArea] private string description = "";
        [SerializeField] private Sprite icon; 
        
        [Header("Inventory Settings")]
        [SerializeField] [Min(1)] private int maxStack = 1;

        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;

        public virtual bool Use(Character user)
        {
            return false;
        }
        public virtual void Drop()
        {
        }
    }
}




