using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Equipment;
using Gameplay.Characters;
using UnityEngine;

namespace UI.Equipment
{
        public class EquipmentPanelUI : MonoBehaviour
    {
        public event System.Action<IActionableItem, Vector3> OnActionMenuRequested;
        public event System.Action OnEquipmentClosed;
        [Header("UI")]
        public GameObject equipmentPanel;

        [Header("Slots")]
        [SerializeField] private List<EquipmentSlotUI> slots = new List<EquipmentSlotUI>();

        private IInventoryHolder _provider;
        private IEquipmentController _equipmentManager;

        public void Initialize(IInventoryHolder provider)
        {
            if (provider == null) return;
            _provider = provider;
            _equipmentManager = provider.EquipmentManager;

            if (_equipmentManager != null)
            {
                _equipmentManager.OnItemEquipped += HandleItemEquipped;
                _equipmentManager.OnItemUnequipped += HandleItemUnequipped;

                foreach (var pair in _equipmentManager.EquippedItems)
                    HandleItemEquipped(pair.Key, pair.Value);
            }

            foreach (var slot in slots.Where(slot => slot != null))
            {
                slot.SetEquipmentManager(_equipmentManager);
                slot.SetActionHandler(provider.Inventory?.ActionHandler);
                slot.OnRightClicked += HandleSlotRightClicked;
            }
        }

        private void Awake()
        {
            if (equipmentPanel == null) equipmentPanel = transform.Find("EquipmentPanel")?.gameObject;
            if (equipmentPanel != null) equipmentPanel.SetActive(false);
        }

        public bool IsVisible => equipmentPanel != null && equipmentPanel.activeSelf;

        public void ToggleUI()
        {
            var currentState = equipmentPanel && equipmentPanel.activeSelf;
            SetVisible(!currentState);
        }

        public void SetVisible(bool visible)
        {
            if (equipmentPanel) equipmentPanel.SetActive(visible);
            
            if (_provider is IUIEventListener uiListener)
                uiListener.OnUIStateChanged("Equipment", visible);

            if (visible) return;
    
            OnEquipmentClosed?.Invoke();
            foreach (var slot in slots.Where(slot => slot))
                slot.SetHighlight(false);
        }

        private void HandleSlotRightClicked(IActionableItem context, Vector3 screenPos)
        {
            OnActionMenuRequested?.Invoke(context, screenPos);
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable equippableItem)
        {
            var slotUI = FindSlotUI(slot);
            if (slotUI == null) return;

            Sprite icon = null;
            if (equippableItem is Data.Equipment.Equipment item) icon = item.Icon;
            slotUI.SetItem(equippableItem, icon);
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            var slotUI = FindSlotUI(slot);
            if (slotUI != null) slotUI.ClearItem();
        }

        private EquipmentSlotUI FindSlotUI(EquipSlot slot)
        {
            return slots.FirstOrDefault(s => s != null && s.SlotType == slot);
        }
    }
}


