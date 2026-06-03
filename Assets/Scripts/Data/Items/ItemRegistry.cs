using System.Collections.Generic;
using UnityEngine;

namespace Data.Items
{
    /// <summary>
    /// Cung cấp cơ chế lookup an toàn (safe lookup) cho ItemData dựa trên ID.
    /// Giải quyết vấn đề Resources.Load cần đường dẫn chính xác (bao gồm cả thư mục con).
    /// </summary>
    public static class ItemRegistry
    {
        private static Dictionary<string, ItemData> _itemCache;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            _itemCache = new Dictionary<string, ItemData>();
            
            // Load tất cả ItemData trong thư mục Resources/Items (và các thư mục con)
            ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
            foreach (var item in allItems)
            {
                if (item != null)
                {
                    string key = item.ID;
                    if (!string.IsNullOrEmpty(key) && !_itemCache.ContainsKey(key))
                    {
                        _itemCache.Add(key, item);
                    }
                }
            }
            
            // Dự phòng: Load cả trong thư mục Data/Items nếu có
            ItemData[] dataItems = Resources.LoadAll<ItemData>("Data/Items");
            foreach (var item in dataItems)
            {
                if (item != null)
                {
                    string key = item.ID;
                    if (!string.IsNullOrEmpty(key) && !_itemCache.ContainsKey(key))
                    {
                        _itemCache.Add(key, item);
                    }
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Lấy ItemData dựa trên ID an toàn mà không cần biết chính xác đường dẫn thư mục con.
        /// </summary>
        public static ItemData GetItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_itemCache.TryGetValue(itemId, out var item))
            {
                return item;
            }

            // Nếu tìm không thấy theo ID tĩnh, có thể cấu hình Enemy ghi sai tên ID.
            // Dự phòng: Tìm theo filename
            foreach (var kvp in _itemCache)
            {
                if (kvp.Value.name == itemId)
                {
                    return kvp.Value;
                }
            }

            Debug.LogWarning($"[ItemRegistry] Cannot find ItemData with ID or filename: {itemId}");
            return null;
        }
    }
}
