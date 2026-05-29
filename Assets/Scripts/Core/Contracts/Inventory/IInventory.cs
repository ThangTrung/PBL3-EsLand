using System;
using System.Collections.Generic;
using Core.Contracts.Shared;
using Data.Items;
namespace Core.Contracts.Inventory
{
    public interface IInventory
    {
        IReadOnlyList<IInventorySlot> Slots { get; }
        int Capacity { get; }
        int UsedSlots { get; }
        event Action OnInventoryChanged;
        void NotifyChanged();
        bool AddItem(ItemData item, int amount = 1);
        bool RemoveItem(ItemData item, int amount = 1);
        void ConsumeSlot(IInventorySlot slot, int amount = 1);
        bool RemoveSlot(IInventorySlot slot);
        int CountItem(ItemData item);
        void Clear();
        IItemActionHandler ActionHandler { get; }
        void SwapSlots(int indexA, int indexB);
    }
}

