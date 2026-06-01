using UnityEngine;

namespace Gameplay.Components
{
    /// <summary>
    /// Component xử lý hiển thị highlight (Outline) cho các đối tượng môi trường.
    /// Tách biệt hoàn toàn phần Hiển thị khỏi phần Logic tài nguyên.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class Highlight : MonoBehaviour
    {
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Material _originalMaterial;
        private Material _fallbackHighlightMaterial;
        private bool _needsMaterialSwap;
        private static readonly int OutlineEnabledHash = Shader.PropertyToID("_OutlineEnabled");

        private void EnsureInitialized()
        {
            if (!_renderer)
            {
                _renderer = GetComponent<Renderer>();
            }
            _propBlock ??= new MaterialPropertyBlock();

            // Tự động kiểm tra xem shader hiện tại có hỗ trợ viền không
            if (_needsMaterialSwap || !_renderer || !_renderer.sharedMaterial) return;
            if (_renderer.sharedMaterial.HasProperty(OutlineEnabledHash)) return;
            _needsMaterialSwap = true;
            if (!_fallbackHighlightMaterial)
            {
                // Tự động tải Material Highlight có viền từ thư mục Resources
                _fallbackHighlightMaterial = Resources.Load<Material>("Materials/Highlight");
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void SetHighlight(bool enabled)
        {
            EnsureInitialized();
            if (_renderer == null) return;

            // Xử lý tráo Material nếu cần thiết
            if (_needsMaterialSwap && _fallbackHighlightMaterial != null)
            {
                if (enabled)
                {
                    if (_originalMaterial == null) _originalMaterial = _renderer.sharedMaterial;
                    _renderer.sharedMaterial = _fallbackHighlightMaterial;
                }
                else
                {
                    if (_originalMaterial != null)
                    {
                        _renderer.sharedMaterial = _originalMaterial;
                        _originalMaterial = null;
                    }
                }
            }

            // Luôn cập nhật PropertyBlock để báo cho Shader biết cần bật hay tắt viền
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(OutlineEnabledHash, enabled ? 1f : 0f);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
