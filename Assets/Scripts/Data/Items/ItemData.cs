using Core.Contracts.Shared;
using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Basic Info")]
        [SerializeField] private string id = "";
        [SerializeField] protected string itemName = "";
        [SerializeField] [TextArea] private string description = "";
        [SerializeField] private Sprite icon; 
        
        [Header("Inventory Settings")]
        [SerializeField] [Min(1)] private int maxStack = 1;
        
        [Header("Cooking Settings")]
        [SerializeField] private float fuelTime = 0f;

        public string ID => id;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;
        public float FuelTime => fuelTime;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
                
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}