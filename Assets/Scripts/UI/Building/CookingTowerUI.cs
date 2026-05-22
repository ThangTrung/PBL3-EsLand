using System;
using Core.Contracts.Inventory;
using Gameplay.Building;
using UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Building
{
    public class CookingTowerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CookingTower currentTower;

        [Header("Slots")]
        [SerializeField] private InventorySlotUI inputSlotUI;
        [SerializeField] private InventorySlotUI fuelSlotUI;
        [SerializeField] private InventorySlotUI outputSlotUI;

        [Header("Progress Bars")]
        [SerializeField] private Image fuelProgressBar;
        [SerializeField] private Image cookingProgressBar;

        public event Action<int, IInventorySlot> OnSlotClicked;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        private void Start()
        {
            if (inputSlotUI) inputSlotUI.Init(0);
            if (fuelSlotUI) fuelSlotUI.Init(1);
            if (outputSlotUI) outputSlotUI.Init(2);

            if (inputSlotUI) inputSlotUI.OnLeftClicked += HandleSlotClicked;
            if (fuelSlotUI) fuelSlotUI.OnLeftClicked += HandleSlotClicked;
            if (outputSlotUI) outputSlotUI.OnLeftClicked += HandleSlotClicked;

            if (currentTower != null)
                BindTower(currentTower);
        }

        private void OnDestroy()
        {
            if (inputSlotUI) inputSlotUI.OnLeftClicked -= HandleSlotClicked;
            if (fuelSlotUI) fuelSlotUI.OnLeftClicked -= HandleSlotClicked;
            if (outputSlotUI) outputSlotUI.OnLeftClicked -= HandleSlotClicked;

            if (currentTower != null)
                currentTower.OnStateChanged -= UpdateUI;
            
            CookingTower.OnTowerInteracted -= HandleTowerInteracted;
        }

        private void Awake()
        {
            CookingTower.OnTowerInteracted += HandleTowerInteracted;
            ClosePanel();
        }

        private void HandleTowerInteracted(CookingTower tower)
        {
            BindTower(tower);
            OpenPanel();
        }

        private void BindTower(CookingTower tower)
        {
            if (currentTower != null)
            {
                currentTower.OnStateChanged -= UpdateUI;
            }

            currentTower = tower;
            if (currentTower != null)
            {
                currentTower.OnStateChanged += UpdateUI;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (currentTower == null) return;

            // Update slots
            if (inputSlotUI) inputSlotUI.Refresh(currentTower.Slots[0]);
            if (fuelSlotUI) fuelSlotUI.Refresh(currentTower.Slots[1]);
            if (outputSlotUI) outputSlotUI.Refresh(currentTower.Slots[2]);

            // Update progress bars
            if (fuelProgressBar != null)
            {
                fuelProgressBar.fillAmount = currentTower.MaxFuelTime > 0 ? (currentTower.CurrentFuelTime / currentTower.MaxFuelTime) : 0f;
            }

            if (cookingProgressBar != null)
            {
                cookingProgressBar.fillAmount = currentTower.SmeltTime > 0 ? (currentTower.CookingProgress / currentTower.SmeltTime) : 0f;
            }
        }

        private void HandleSlotClicked(int slotIndex, IInventorySlot slotData)
        {
            OnSlotClicked?.Invoke(slotIndex, slotData);
        }

        public void OpenPanel()
        {
            if (panelRoot) panelRoot.SetActive(true);
            UpdateUI();
        }

        public void ClosePanel()
        {
            if (panelRoot) panelRoot.SetActive(false);
        }
    }
}
