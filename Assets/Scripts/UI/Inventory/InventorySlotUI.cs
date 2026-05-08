using System;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using TMPro;
using UI.ItemActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] public Image icon;
        [SerializeField] public TextMeshProUGUI amountText;
        [SerializeField] public Image durabilityBar;
        [SerializeField] public Image highlightImage;
        [SerializeField] public Image equippedOverlay;
        [SerializeField] public GameObject tooltipRoot;
        [SerializeField] public TextMeshProUGUI tooltipName;
        [SerializeField] public TextMeshProUGUI tooltipDesc;

        [Header("Actions")]
        [SerializeField] public MonoBehaviour actionMenuProvider;

        private IInventorySlot _slotData;
        private IItemActionHandler _actionHandler;
        public int SlotIndex { get; private set; }

        public event Action<IActionableItem, Vector3> OnRightClicked;
        public void Init(int index, IItemActionHandler actionHandler = null)
        {
            SlotIndex = index;
            _actionHandler = actionHandler;
        }

        public void Refresh(IInventorySlot slotData)
        {
            _slotData = slotData;

            if (GetComponent<Image>() != null) 
                GetComponent<Image>().enabled = true;
            
            if (_slotData == null || _slotData.IsEmpty)
            {
                ClearVisuals();
                return;
            }

            icon.sprite = _slotData.Item.Icon;
            icon.enabled = true;

            amountText.enabled = _slotData.Amount > 1;
            if (_slotData.Amount > 1)
                amountText.text = _slotData.Amount.ToString();

            var hasDurability = _slotData.Item is IDurable;
            if (durabilityBar) durabilityBar.gameObject.SetActive(hasDurability);
            if (hasDurability && durabilityBar)
                durabilityBar.fillAmount = _slotData.DurabilityPercent;

            if (!equippedOverlay) return;
            var isEquipped = _actionHandler?.IsEquipped(_slotData) ?? false;
            equippedOverlay.enabled = isEquipped;
        }

        private void ClearVisuals()
        {
            icon.sprite = null;
            icon.enabled = false;
            amountText.enabled = false;
            if (durabilityBar) durabilityBar.gameObject.SetActive(false);
            if (equippedOverlay) equippedOverlay.enabled = false;
            HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)  
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    break;
                case PointerEventData.InputButton.Right:
                {
                    HideTooltip();
                    if (_slotData == null || _slotData.IsEmpty) return;
                    var context = new InventorySlotActionContext(_slotData, _actionHandler);
                    OnRightClicked?.Invoke(context, eventData.position);
                    break;
                }
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHighlight(bool active)
        {
            highlightImage.enabled = active;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
            if (_slotData is { IsEmpty: false }) ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltipRoot == null || _slotData == null) return;
            tooltipRoot.SetActive(true);
            if (tooltipName != null) tooltipName.text = _slotData.Item.ItemName;
            if (tooltipDesc != null) tooltipDesc.text = _slotData.Item.Description;
        }

        private void HideTooltip()
        {
            if (tooltipRoot) tooltipRoot.SetActive(false);
        }

        public void ResetState()
        {
            if (highlightImage != null) highlightImage.enabled = false;
            HideTooltip();
        }
    }
}


