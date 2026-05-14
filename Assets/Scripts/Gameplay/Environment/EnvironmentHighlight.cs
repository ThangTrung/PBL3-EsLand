using UnityEngine;

namespace Gameplay.Environment
{
    /// <summary>
    /// Component xử lý hiển thị highlight (Outline) cho các đối tượng môi trường.
    /// Tách biệt hoàn toàn phần Hiển thị khỏi phần Logic tài nguyên.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class EnvironmentHighlight : MonoBehaviour
    {
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private static readonly int OutlineEnabledHash = Shader.PropertyToID("_OutlineEnabled");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        private void OnMouseEnter()
        {
            SetHighlight(true);
        }

        private void OnMouseExit()
        {
            SetHighlight(false);
        }

        private void SetHighlight(bool enabled)
        {
            if (_renderer == null) return;
            
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(OutlineEnabledHash, enabled ? 1f : 0f);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
