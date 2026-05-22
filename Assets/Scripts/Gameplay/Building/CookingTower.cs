using System;
using Data.Items;
using Gameplay.Characters;
using Gameplay.Inventory;
using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Building
{
    public class CookingTower : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float interactRange = 3f;

        public InventorySlot[] Slots { get; } = new InventorySlot[3];

        public float CurrentFuelTime { get; private set; }
        public float MaxFuelTime { get; private set; }
        public float CookingProgress { get; private set; }
        public float SmeltTime { get; private set; }

        public event Action OnStateChanged;
        public static event Action<CookingTower> OnTowerInteracted;

        private void Awake()
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new InventorySlot(null, 0);
            }
        }

        public void Interact(Character interactor)
        {
            if (Vector2.Distance(transform.position, interactor.transform.position) <= interactRange)
            {
                Debug.Log($"[CookingTower] {interactor.CharacterName} tương tác thành công.");
                OnTowerInteracted?.Invoke(this);
            }
            else
            {
                Debug.Log($"[CookingTower] {interactor.CharacterName} đứng quá xa để tương tác.");
            }
        }

        private void Update()
        {
            var isDirty = false;
            var isCooking = false;

            var hasValidInput = false;
            MaterialItem inputMaterial = null;

            // 1. Kiểm tra Slot Input (0)
            if (!Slots[0].IsEmpty && Slots[0].ItemData is MaterialItem mat && mat.canBeSmelted)
            {
                if (CanSmelt(mat))
                {
                    hasValidInput = true;
                    inputMaterial = mat;
                    SmeltTime = mat.smeltTime;
                }
            }

            if (!hasValidInput)
            {
                if (CookingProgress > 0)
                {
                    CookingProgress = 0;
                    isDirty = true;
                }
            }
            else
            {
                // 2. Tiêu thụ Fuel nếu cần (Slot 1)
                if (CurrentFuelTime <= 0)
                {
                    if (!Slots[1].IsEmpty && Slots[1].ItemData.FuelTime > 0)
                    {
                        CurrentFuelTime = Slots[1].ItemData.FuelTime;
                        MaxFuelTime = CurrentFuelTime;
                        Slots[1].AddAmount(-1);
                        if (Slots[1].Amount <= 0) Slots[1].Clear();
                        isDirty = true;
                    }
                }

                // 3. Tiến hành nấu
                if (CurrentFuelTime > 0)
                {
                    CurrentFuelTime -= Time.deltaTime;
                    if (CurrentFuelTime < 0) CurrentFuelTime = 0;
                    
                    CookingProgress += Time.deltaTime;
                    isCooking = true;
                    isDirty = true; // Update progress bar liên tục

                    if (CookingProgress >= inputMaterial.smeltTime)
                    {
                        CookingProgress = 0;
                        
                        // Trừ Input
                        Slots[0].AddAmount(-1);
                        if (Slots[0].Amount <= 0) Slots[0].Clear();

                        // Thêm Output (Slot 2)
                        if (Slots[2].IsEmpty)
                        {
                            Slots[2].SetItem(inputMaterial.resultItem, 1);
                        }
                        else if (Slots[2].ItemData.ID == inputMaterial.resultItem.ID)
                        {
                            Slots[2].AddAmount(1);
                        }
                    }
                }
            }
            
            if (!isCooking && CurrentFuelTime > 0)
            {
                CurrentFuelTime -= Time.deltaTime;
                if (CurrentFuelTime < 0) CurrentFuelTime = 0;
                isDirty = true;
            }

            if (isDirty)
            {
                OnStateChanged?.Invoke();
            }
        }

        private bool CanSmelt(MaterialItem inputMaterial)
        {
            if (Slots[2].IsEmpty) return true;
            if (Slots[2].ItemData.ID != inputMaterial.resultItem.ID) return false; 
            return Slots[2].Amount < Slots[2].ItemData.MaxStack;
        }

        public bool TryAddItem(int slotIndex, ItemData item, int amount)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length) return false;
            
            var targetSlot = Slots[slotIndex];
            if (targetSlot.IsEmpty)
            {
                targetSlot.SetItem(item, amount);
                OnStateChanged?.Invoke();
                return true;
            }

            if (targetSlot.ItemData.ID != item.ID || targetSlot.Amount + amount > item.MaxStack) return false;
            targetSlot.AddAmount(amount);
            OnStateChanged?.Invoke();
            return true;
        }

        public ItemData WithdrawItem(int slotIndex, out int amount)
        {
            amount = 0;
            if (slotIndex < 0 || slotIndex >= Slots.Length) return null;

            var targetSlot = Slots[slotIndex];
            if (targetSlot.IsEmpty) return null;

            var item = targetSlot.ItemData;
            amount = targetSlot.Amount;
            
            targetSlot.Clear();
            OnStateChanged?.Invoke();
            return item;
        }
        
        public void SetTestItem(int slotIndex, ItemData item, int amount)
        {
            Slots[slotIndex].SetItem(item, amount);
            OnStateChanged?.Invoke();
        }
    }
}
