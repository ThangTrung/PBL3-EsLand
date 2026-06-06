using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các loại Slot trong game (Inventory, Equipment, Crafting).
    /// Hợp nhất logic hiển thị cơ bản và tích hợp Tooltip tập trung.
    /// </summary>
    public abstract class SlotUIBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Common Visuals")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TextMeshProUGUI amountText;
        [SerializeField] protected Image highlightOverlay;

        protected string _cachedTitle;
        protected string _cachedContent;
        protected bool _hasData;

        public virtual void SetHighlight(bool active)
        {
            if (highlightOverlay != null) highlightOverlay.enabled = active;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
            if (_hasData && GlobalTooltipUI.Instance != null)
            {
                GlobalTooltipUI.Instance.Show(_cachedTitle, _cachedContent);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
            if (GlobalTooltipUI.Instance != null)
            {
                GlobalTooltipUI.Instance.Hide();
            }
        }

        protected virtual void OnDisable()
        {
            // Tự động ẩn tooltip nếu slot bị ẩn bất ngờ
            if (GlobalTooltipUI.Instance != null) GlobalTooltipUI.Instance.Hide();
        }

        protected virtual void ClearVisuals()
        {
            _hasData = false;
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            if (amountText != null) amountText.enabled = false;
        }
    }
}
