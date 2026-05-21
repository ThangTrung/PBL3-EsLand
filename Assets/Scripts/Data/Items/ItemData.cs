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

        public string ID => string.IsNullOrEmpty(id) ? name : id;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;
        public float FuelTime => fuelTime;

        private void OnValidate()
        {
            // Nếu ô id khác với tên file Asset, tự động lấy tên file gán vào luôn
            if (id == name) return;
            id = name;
                
#if UNITY_EDITOR
            // Báo cho Unity biết là file này đã bị thay đổi để nó lưu lại code mới sinh
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}