using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;
using Gameplay.Characters;
using UI.ItemActions;
using UnityEngine;

namespace UI.Inventory
{
    public class InventoryPanelUI : MonoBehaviour
    {
        public event Action<IActionableItem, Vector3> OnActionMenuRequested;
        public event Action OnInventoryClosed;
        
        public event Action<int, IInventorySlot> OnSlotLeftClicked;
        public event Action<int, IInventorySlot> OnSlotRightClickedEvent;

        private IInventory _inventory;

        [Header("UI References")]
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Audio")]
        [SerializeField] private AudioData _openSound;
        [SerializeField] private AudioData _closeSound;

        private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();
        private const int SelectedSlotIndex = -1;

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
            // Tránh phát âm thanh khi khởi tạo hoặc nếu trạng thái không đổi
            bool stateChanged = IsVisible != visible;
            
            IsVisible = visible;
            if (canvasRoot)
            {
                canvasRoot.SetActive(visible);
            }
                
            if (_provider is Core.Contracts.Shared.IUIEventListener uiListener)
                uiListener.OnUIStateChanged("Inventory", visible);

            // Phát âm thanh
            if (stateChanged && AudioManager.Instance != null)
            {
                var soundToPlay = visible ? _openSound : _closeSound;
                if (soundToPlay != null)
                {
                    AudioManager.Instance.PlaySFX(soundToPlay);
                }
            }

            if (visible) return;
    
            OnInventoryClosed?.Invoke(); 
            ResetAllSlots();
    
            if (UnityEngine.EventSystems.EventSystem.current)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        
        private void ResetAllSlots()
        {
            foreach (var slot in _slotUIs.Where(slot => slot))
            {
                slot.ResetState();
            }
        }

        private void Update()
        {
            if (!IsVisible) return;
            HandleNavigation();
        }

        private void HandleNavigation()
        {
            if (SelectedSlotIndex < 0 || SelectedSlotIndex >= _slotUIs.Count) return;
            var slotUI = _slotUIs[SelectedSlotIndex];
            var slots = _inventory?.Slots;
            if (slots == null || SelectedSlotIndex >= slots.Count || slots[SelectedSlotIndex].IsEmpty) return;
            
            var context = new InventorySlotActionContext(slots[SelectedSlotIndex], _inventory.ActionHandler);
            OnActionMenuRequested?.Invoke(context, slotUI.transform.position); 
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
                var index = i;
                var go = Instantiate(slotPrefab, slotsContainer);
                go.transform.localScale = Vector3.one;
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (!slotUI) continue;
                slotUI.Init(index, _inventory.ActionHandler);
                
                slotUI.OnLeftClicked += (idx, data) => OnSlotLeftClicked?.Invoke(idx, data);
                slotUI.OnRightClicked += HandleSlotRightClicked;
                slotUI.OnRightClicked += (context, pos) => OnSlotRightClickedEvent?.Invoke(index, _inventory.Slots[index]);
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
            if (SelectedSlotIndex >= 0 && SelectedSlotIndex < _slotUIs.Count)
                _slotUIs[SelectedSlotIndex].SetHighlight(true);
        }
    }
}