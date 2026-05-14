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

        public void SetItem(IEquippable item, Sprite itemSprite)
        {
            _currentItem = item;
            if (icon == null) return;
            
            if (item != null && itemSprite != null)
            {
                icon.sprite = itemSprite;
                icon.color = new Color(1, 1, 1, 1f);
            }
            else ClearVisuals();
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
            _defaultColor.a = slotType == EquipSlot.MainHand ? 0f : 0.2f;
            icon.color = _defaultColor;
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (highlightImage) highlightImage.enabled = isHighlighted;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _currentItem == null) return;
            var context = new EquipmentSlotActionContext(_equipmentManager, slotType, _currentItem);
            OnRightClicked?.Invoke(context, eventData.position);
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHighlight(true);
        public void OnPointerExit(PointerEventData eventData) => SetHighlight(false);
    }
}
