using System;
using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Data.Equipment;
using UI.ItemActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Equipment
{
    public class EquipmentSlotUI : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Config")]
        [SerializeField] private EquipSlot slotType;
        public EquipSlot SlotType => slotType;

        [Header("References")]
        [SerializeField] private Image icon;
        [SerializeField] private Image highlightImage;

        public event Action<IActionableItem, Vector3> OnRightClicked;

        private IEquippable _currentItem;
                private IEquipmentController _equipmentManager;
        private IItemActionHandler _actionHandler;
        private Sprite _defaultSprite;
        private Color _defaultColor;

        private void Awake()
        {
            if (icon != null)
            {
                _defaultSprite = icon.sprite;
                _defaultColor = icon.color;
            }
            SetHighlight(false); 
            if (_currentItem == null)
            {
                ClearVisuals();
            }
        }

        public void SetEquipmentManager(IEquipmentController manager) => _equipmentManager = manager;

        public void SetActionHandler(IItemActionHandler handler) => _actionHandler = handler;


        public void SetItem(IEquippable item, Sprite itemSprite)
        {
            _currentItem = item;
            if (icon == null) return;
            
            if (item != null)
            {
                // Cập nhật icon cho mọi loại trang bị nếu có sprite
                if (itemSprite != null)
                {
                    if (slotType == EquipSlot.MainHand)
                    {
                        icon.sprite = itemSprite;
                    }
                    icon.enabled = true;
                }
                
                // Luôn set alpha lên 1 khi có đồ
                icon.color = Color.white;
            }
            else 
            {
                ClearVisuals();
            }
        }
        

        public void ClearItem()
        {
            _currentItem = null;
            ClearVisuals();
        }

        private void ClearVisuals()
        {
            if (icon == null) return;
            
            icon.sprite = _defaultSprite;
            
            // Nếu là MainHand và không có default sprite thì ẩn luôn image
            if (slotType == EquipSlot.MainHand && _defaultSprite == null)
            {
                icon.enabled = false;
            }
            else
            {
                icon.enabled = _defaultSprite != null;
                // Sử dụng bản sao để không ghi đè vào field _defaultColor
                Color c = _defaultColor;
                c.a = slotType == EquipSlot.MainHand ? 0f : 0.2f;
                icon.color = c;
            }
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (highlightImage) highlightImage.enabled = isHighlighted;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _currentItem == null) return;
            var context = new EquipmentSlotActionContext(_actionHandler, slotType, _currentItem);
            OnRightClicked?.Invoke(context, eventData.position);
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHighlight(true);
        public void OnPointerExit(PointerEventData eventData) => SetHighlight(false);
    }
}
