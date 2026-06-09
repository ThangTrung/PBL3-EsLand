using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Cursor
{
    /// <summary>
    /// Gắn vào các UI elements để đổi con trỏ chuột khi hover.
    /// Tự động hiển thị dấu cấm nếu Selectable (Button) đang bị disable.
    /// </summary>
    public class UICursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CursorType hoverType = CursorType.Pointer;
        [SerializeField] private CursorType disabledType = CursorType.Forbidden;

        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CursorManager.Instance == null) return;

            // Nếu có component Selectable (Button, Toggle...), kiểm tra trạng thái interactable
            if (_selectable != null && !_selectable.interactable)
            {
                CursorManager.Instance.SetCursor(disabledType);
            }
            else
            {
                CursorManager.Instance.SetCursor(hoverType);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetNormalCursor();
            }
        }
        
        private void OnDisable()
        {
            // Reset cursor nếu UI bị ẩn đi khi đang hover
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetNormalCursor();
            }
        }
    }
}
