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
    public class EquipmentSlotUI : SlotUIBase, IPointerClickHandler
    {
        [Header("Slot Config")]
        [SerializeField] private EquipSlot slotType;
        public EquipSlot SlotType => slotType;

        public event Action<IActionableItem, Vector3> OnRightClicked;

        private IEquippable _currentItem;
        private IItemActionHandler _actionHandler;
        private Sprite _defaultSprite;
        private Color _defaultColor;

        private void Awake()
        {
            if (iconImage != null)
            {
                _defaultSprite = iconImage.sprite;
                _defaultColor = iconImage.color;
            }
            SetHighlight(false); 
            if (_currentItem == null)
            {
                ClearVisuals();
            }
        }

        public void SetEquipmentManager(IEquipmentController manager) { }

        public void SetActionHandler(IItemActionHandler handler) => _actionHandler = handler;

        public void SetItem(IEquippable item, Sprite itemSprite)
        {
            _currentItem = item;
            if (iconImage == null) return;
            
            if (item != null)
            {
                _hasData = true;
                if (item is Data.Equipment.Equipment data)
                {
                    _cachedTitle = data.ItemName;
                    _cachedContent = data.Description;
                }

                if (itemSprite != null)
                {
                    if (slotType == EquipSlot.MainHand)
                    {
                        iconImage.sprite = itemSprite;
                    }
                    iconImage.enabled = true;
                }
                
                iconImage.color = Color.white;
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

        protected override void ClearVisuals()
        {
            base.ClearVisuals();
            if (iconImage == null) return;
            
            iconImage.sprite = _defaultSprite;
            
            if (slotType == EquipSlot.MainHand && _defaultSprite == null)
            {
                iconImage.enabled = false;
            }
            else
            {
                iconImage.enabled = _defaultSprite != null;
                Color c = _defaultColor;
                c.a = slotType == EquipSlot.MainHand ? 0f : 0.2f;
                iconImage.color = c;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _currentItem == null) return;
            if (GlobalTooltipUI.Instance != null) GlobalTooltipUI.Instance.Hide();
            
            var context = new EquipmentSlotActionContext(_actionHandler, slotType, _currentItem);
            OnRightClicked?.Invoke(context, eventData.position);
        }
    }
}
