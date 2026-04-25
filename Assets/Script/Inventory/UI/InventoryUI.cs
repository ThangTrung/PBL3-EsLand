using System.Collections.Generic;
using UnityEngine;
using Script.Interfaces;
using Script.Inventory.Controller;

namespace Script.Inventory.UI
{
    public class InventoryUI : MonoBehaviour, IInventoryUI
    {
        [Header("System References")]
        [SerializeField] private MonoBehaviour inventoryProvider;
        private IInventory Inventory => inventoryProvider as IInventory;
        
        [SerializeField] private ItemActionMenu actionMenu;

        [Header("UI References")]
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Settings")]
        [SerializeField] private int columns = 8;

        private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();
        private int _selectedSlotIndex = -1;

        public bool IsVisible => canvasRoot.activeSelf;

        public void SetInventoryProvider(MonoBehaviour provider)
        {
            inventoryProvider = provider;

            if (Inventory == null) 
                return;
            Inventory.OnInventoryChanged += RefreshUI;
            BuildSlots();
            RefreshUI();
        }

        private void Start()
        {
            if (Inventory != null)
            {
                Inventory.OnInventoryChanged += RefreshUI;
                BuildSlots();
            }
            SetVisible(false); 
        }

        private void OnDestroy()
        {
            if (Inventory != null)
                Inventory.OnInventoryChanged -= RefreshUI;
        }
        
        public void SetVisible(bool visible)
        {
            canvasRoot.SetActive(visible);

            if (visible)
                return;
            actionMenu.HideMenu();
            ClearAllHighlights();
            if (UnityEngine.EventSystems.EventSystem.current)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            if (!IsVisible) return;
            HandleNavigation();
        }

        private void HandleNavigation()
        {
            if (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.Return)) 
                return;
            // Open menu for selected slot via keyboard
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _slotUIs.Count) 
                return;
            var slotUI = _slotUIs[_selectedSlotIndex];
            var slots = Inventory?.Slots;
            if (slots == null || _selectedSlotIndex >= slots.Count || slots[_selectedSlotIndex].IsEmpty) 
                return;
            if (actionMenu) actionMenu.ShowMenu(slots[_selectedSlotIndex], slotUI.transform.position);
        }

        public void SelectSlot(int index)
        {
            if (_slotUIs == null || _slotUIs.Count == 0) 
                return;
            index = Mathf.Clamp(index, 0, _slotUIs.Count - 1);
            ClearAllHighlights();
            _selectedSlotIndex = index;
            _slotUIs[_selectedSlotIndex].SetHighlight(true);
        }

        private void ClearAllHighlights()
        {
            foreach (var slot in _slotUIs)
                slot.SetHighlight(false);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void BuildSlots()
        {
            if (!slotsContainer || Inventory == null) return;

            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);
            _slotUIs.Clear();

            var cap = Inventory.Capacity;
            for (var i = 0; i < cap; i++)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (!slotUI) 
                    continue;
                slotUI.Init(i);
                slotUI.OnRightClicked += HandleSlotRightClicked;
                slotUI.OnHovered += SelectSlot;
                _slotUIs.Add(slotUI);
            }
        }
        
        private void HandleSlotRightClicked(IInventorySlot slotData, Vector3 pos)
        {
            if (actionMenu) actionMenu.ShowMenu(slotData, pos);
        }

        public void RefreshUI()
        {
            if (Inventory == null) return;
            var slots = Inventory.Slots;

            if (_slotUIs.Count != Inventory.Capacity)
                BuildSlots();

            for (var i = 0; i < _slotUIs.Count; i++)
            {
                var data = (i < slots.Count) ? slots[i] : null;
                _slotUIs[i].Refresh(data);
            }
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slotUIs.Count)
            {
                _slotUIs[_selectedSlotIndex].SetHighlight(true);
            }
        }
    }
}