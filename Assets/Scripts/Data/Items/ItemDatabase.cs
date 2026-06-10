using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data.Items
{
    /// <summary>
    /// Centralized database for all ItemData assets.
    /// Provides fast lookup by ID and prevents redundant resource loading.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        private static ItemDatabase _instance;
        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                    if (_instance != null) _instance.Initialize();
                }
                return _instance;
            }
        }

        [SerializeField] private List<ItemData> allItems = new List<ItemData>();
        private Dictionary<string, ItemData> _itemLookup = new Dictionary<string, ItemData>();

        public void Initialize()
        {
            _itemLookup.Clear();
            foreach (var item in allItems)
            {
                if (item == null) continue;

                if (string.IsNullOrEmpty(item.ID))
                {
                    Debug.LogError($"[ItemDatabase] Item '{item.name}' has a NULL or EMPTY ID! Please assign a GUID in the inspector.");
                    continue;
                }

                if (_itemLookup.ContainsKey(item.ID))
                {
                    Debug.LogError($"[ItemDatabase] DUPLICATE ID FOUND: '{item.ID}' is used by both '{_itemLookup[item.ID].name}' and '{item.name}'. IDs must be unique!");
                    continue;
                }

                _itemLookup.Add(item.ID, item);
            }
            Debug.Log($"[ItemDatabase] Successfully indexed {_itemLookup.Count} items.");
        }

        public ItemData GetItemByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            if (_itemLookup.Count == 0 && allItems.Count > 0) Initialize();
            
            // 1. Tìm theo GUID (Ưu tiên)
            if (_itemLookup.TryGetValue(id, out var item))
            {
                return item;
            }

            // 2. Fallback: Tìm theo Tên file (Dành cho các bản save cũ lưu "Axe", "Defense"...)
            var fallbackItem = allItems.FirstOrDefault(i => i.name == id || i.ItemName == id);
            if (fallbackItem != null)
            {
                Debug.Log($"[ItemDatabase] Fallback found item '{id}' by name instead of ID.");
                return fallbackItem;
            }

            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Database")]
        public void RefreshDatabase()
        {
            allItems = Resources.LoadAll<ItemData>("Items").ToList();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"ItemDatabase: Indexed {allItems.Count} items.");
        }
#endif
    }
}
