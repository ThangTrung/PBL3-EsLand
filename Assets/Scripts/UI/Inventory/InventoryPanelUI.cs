using System;
using System.Collections.Generic;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Gameplay.Characters;
using UI.ItemActions;
using UnityEngine;

namespace UI.Inventory
{
    public class InventoryPanelUI : MonoBehaviour
    {
        public event Action<IActionableItem, Vector3> OnActionMenuRequested;
        public event Action OnInventoryClosed;

        private IInventory _inventory;

        [Header("UI References")]
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Settings")]
        [SerializeField] private int columns = 8;

        private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();
        private int _selectedSlotIndex = -1;

        public bool IsVisible { get; private set; }

        private IInventoryHolder _provider;

        public void Initialize(IInventoryHolder provider)
        {
            if (provider == null) return;

            _provider = provider;
            _inventory = provider.Inventory;

            if (_inventory == null) return;

            _inventory.OnInventoryChanged += RefreshUI;

            BuildSlots();
            RefreshUI();
        }

        private void Awake()
        {
            if (canvasRoot == null) canvasRoot = transform.Find("CanvasRoot")?.gameObject;
        }

        private void Start()
        {
            SetVisible(false);
        }

        public void ToggleUI()
        {
            var currentState = canvasRoot && canvasRoot.activeSelf;
            SetVisible(!currentState);
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            if (canvasRoot)
            {
                canvasRoot.SetActive(visible);
            }
                
            if (_provider is Player player)
                player.SetInventoryState(visible);

            if (visible) return;
    
            OnInventoryClosed?.Invoke(); 
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
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _slotUIs.Count) return;
            var slotUI = _slotUIs[_selectedSlotIndex];
            var slots = _inventory?.Slots;
            if (slots == null || _selectedSlotIndex >= slots.Count || slots[_selectedSlotIndex].IsEmpty) return;
            
            var context = new InventorySlotActionContext(slots[_selectedSlotIndex], _inventory.ActionHandler);
            OnActionMenuRequested?.Invoke(context, slotUI.transform.position); 
        }

        public void SelectSlot(int index)
        {
            if (_slotUIs == null || _slotUIs.Count == 0) return;
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

        private void BuildSlots()
        {
            if (!slotsContainer || _inventory == null) return;

            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);
            _slotUIs.Clear();

            var cap = _inventory.Capacity;
            for (var i = 0; i < cap; i++)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (!slotUI) continue;
                slotUI.Init(i, _inventory.ActionHandler);
                
                slotUI.OnRightClicked += HandleSlotRightClicked;
                _slotUIs.Add(slotUI);
            }
        }
        
        private void HandleSlotRightClicked(IActionableItem context, Vector3 pos)
        {
            OnActionMenuRequested?.Invoke(context, pos);
        }

        private void RefreshUI()
        {
            if (_inventory == null) return;
            var slots = _inventory.Slots;

            if (_slotUIs.Count != _inventory.Capacity)
                BuildSlots();

            for (var i = 0; i < _slotUIs.Count; i++)
            {
                var data = (i < slots.Count) ? slots[i] : null;
                _slotUIs[i].Refresh(data);
            }
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slotUIs.Count)
                _slotUIs[_selectedSlotIndex].SetHighlight(true);
        }
    }
}