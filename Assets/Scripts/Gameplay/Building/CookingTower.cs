using System;
using System.Collections.Generic;
using Data.Building;
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
        [SerializeField] private List<CookingRecipe> availableRecipes = new List<CookingRecipe>();

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

        public bool CanInteract(Character interactor)
        {
            return true;
        }

        public float GetStaminaCost(Character interactor)
        {
            return 0f;
        }

        public void Interact(Character interactor)
        {
            if (Vector2.Distance(transform.position, interactor.transform.position) <= interactRange)
            {
                OnTowerInteracted?.Invoke(this);
            }
            else
            {
            }
        }

        private void Update()
        {
            var isDirty = false;
            var isCooking = false;

            var hasValidInput = false;
            CookingRecipe currentRecipe = null;

            // 1. Kiểm tra Slot Input (0)
            if (!Slots[0].IsEmpty)
            {
                currentRecipe = GetRecipe(Slots[0].ItemData);
                if (currentRecipe != null && CanCook(currentRecipe))
                {
                    hasValidInput = true;
                    SmeltTime = currentRecipe.CookingTime;
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

                    if (CookingProgress >= currentRecipe.CookingTime)
                    {
                        CookingProgress = 0;
                        
                        // Trừ Input
                        Slots[0].AddAmount(-1);
                        if (Slots[0].Amount <= 0) Slots[0].Clear();

                        // Thêm Output (Slot 2)
                        if (Slots[2].IsEmpty)
                        {
                            Slots[2].SetItem(currentRecipe.OutputItem, 1);
                        }
                        else if (Slots[2].ItemData.ID == currentRecipe.OutputItem.ID)
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

        private CookingRecipe GetRecipe(ItemData inputItem)
        {
            foreach (var recipe in availableRecipes)
            {
                if (recipe != null && recipe.InputItem != null && recipe.InputItem.ID == inputItem.ID)
                {
                    return recipe;
                }
            }
            return null;
        }

        private bool CanCook(CookingRecipe recipe)
        {
            if (recipe == null || recipe.OutputItem == null) return false;
            if (Slots[2].IsEmpty) return true;
            if (Slots[2].ItemData.ID != recipe.OutputItem.ID) return false; 
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
