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
            _itemLookup = allItems.ToDictionary(item => item.ID, item => item);
        }

        public ItemData GetItemByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            if (_itemLookup.Count == 0 && allItems.Count > 0) Initialize();
            
            _itemLookup.TryGetValue(id, out var item);
            return item;
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
