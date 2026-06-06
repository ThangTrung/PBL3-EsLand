using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Infrastructure.Pooling
{
    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjectPoolManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ObjectPoolManager");
                        _instance = go.AddComponent<ObjectPoolManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<GameObject, IObjectPool<GameObject>> _pools = new Dictionary<GameObject, IObjectPool<GameObject>>();
        private Dictionary<GameObject, GameObject> _instanceToPrefab = new Dictionary<GameObject, GameObject>();

        public void InitPool(GameObject prefab, int initialSize = 10, int maxSize = 100)
        {
            if (prefab == null) return;
            
            if (!_pools.ContainsKey(prefab))
            {
                var pool = new ObjectPool<GameObject>(
                    createFunc: () => {
                        var obj = Instantiate(prefab, this.transform);
                        _instanceToPrefab[obj] = prefab;
                        return obj;
                    },
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => {
                        if (_instanceToPrefab.ContainsKey(obj))
                            _instanceToPrefab.Remove(obj);
                        Destroy(obj);
                    },
                    collectionCheck: true,
                    defaultCapacity: initialSize,
                    maxSize: maxSize
                );

                _pools[prefab] = pool;

                // Pre-warm
                GameObject[] prewarmed = new GameObject[initialSize];
                for (int i = 0; i < initialSize; i++)
                {
                    prewarmed[i] = pool.Get();
                }
                for (int i = 0; i < initialSize; i++)
                {
                    pool.Release(prewarmed[i]);
                }
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Get(prefab, position, Quaternion.identity, parent);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;

            if (!_pools.ContainsKey(prefab))
            {
                InitPool(prefab);
            }

            var obj = _pools[prefab].Get();
            
            // [FIX] Cập nhật parent để hỗ trợ kế thừa Elevation Layer
            // [FIX UI SHRINK] Sử dụng worldPositionStays = false để tránh việc Unity tự tính toán lại scale
            // gây ra lỗi tích tụ khiến UI bị thu nhỏ dần mỗi lần lấy từ Pool.
            obj.transform.SetParent(parent, false);
            
            // Đảm bảo scale luôn là 1 (hoặc theo prefab) khi lấy ra khỏi pool
            obj.transform.localScale = Vector3.one;
            
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        public void ReturnToPool(GameObject instance)
        {
            if (instance == null) return;

            if (_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                if (_pools.TryGetValue(prefab, out var pool))
                {
                    pool.Release(instance);
                    return;
                }
            }
            
            // Fallback if not from pool
            Destroy(instance);
        }

        // Keep alias for compatibility
        public void Return(GameObject instance) => ReturnToPool(instance);
    }
}
