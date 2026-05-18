using Core.Contracts.Shared;
using Gameplay.Characters;
using UnityEngine;

namespace Data.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Basic Info")]
        [SerializeField] private string id = ""; // Unique ID for saving/loading and comparison
        [SerializeField] protected string itemName = "";
        [SerializeField] [TextArea] private string description = "";
        [SerializeField] private Sprite icon; 
        
        [Header("Inventory Settings")]
        [SerializeField] [Min(1)] private int maxStack = 1;

        public string ID => string.IsNullOrEmpty(id) ? name : id;
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
        
        private void OnValidate()
        {
            // Nếu ô id khác với tên file Asset, tự động lấy tên file gán vào luôn
            if (id != name)
            {
                id = name;
                
#if UNITY_EDITOR
                // Báo cho Unity biết là file này đã bị thay đổi để nó lưu lại code mới sinh
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}