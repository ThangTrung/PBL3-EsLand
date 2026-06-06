using UnityEngine;
using System.Collections.Generic;

namespace Infrastructure.Performance
{
    /// <summary>
    /// Gắn vào các vật thể cần tối ưu hiệu năng.
    /// Tự động bật/tắt các linh kiện dựa trên lệnh từ PerformanceManager.
    /// </summary>
    public class CullableEntity : MonoBehaviour
    {
        private Renderer[] _renderers;
        private Collider2D[] _colliders;
        private Animator _animator;
        private MonoBehaviour[] _logicScripts;

        public bool IsCulled { get; private set; }

        private void Awake()
        {
            // Cache các component để tránh gọi GetComponent ở Runtime
            _renderers = GetComponentsInChildren<Renderer>(true);
            _colliders = GetComponentsInChildren<Collider2D>(true);
            _animator = GetComponent<Animator>();
            
            // Tìm các script logic cần tắt (Ví dụ các script quản lý AI hoặc Resource)
            // Lưu ý: Không tắt chính script này!
            var allScripts = GetComponentsInChildren<MonoBehaviour>(true);
            var filtered = new List<MonoBehaviour>();
            foreach (var script in allScripts)
            {
                if (script != this && !(script is Infrastructure.SaveSystem.Core.SaveableEntity))
                {
                    filtered.Add(script);
                }
            }
            _logicScripts = filtered.ToArray();
        }

        private void OnEnable()
        {
            if (PerformanceManager.Instance != null)
            {
                PerformanceManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (PerformanceManager.Instance != null)
            {
                PerformanceManager.Instance.Unregister(this);
            }
        }

        /// <summary>
        /// Bật hoặc tắt các component dựa trên trạng thái culling.
        /// </summary>
        public void SetCullState(bool culled)
        {
            if (IsCulled == culled) return;
            IsCulled = culled;

            bool activeState = !culled;

            // Bật/Tắt hiển thị
            foreach (var r in _renderers)
            {
                if (r != null) r.enabled = activeState;
            }

            // Bật/Tắt vật lý
            foreach (var c in _colliders)
            {
                if (c != null) c.enabled = activeState;
            }

            // Bật/Tắt Animator
            if (_animator != null)
            {
                _animator.enabled = activeState;
            }

            // Bật/Tắt Logic (AI, ResourceNode, v.v.)
            foreach (var s in _logicScripts)
            {
                if (s != null) s.enabled = activeState;
            }
        }
    }
}
