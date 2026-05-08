using System;
using System.Collections.Generic;

namespace Script.Inventory.Interfaces
{
    public interface IInventory
    {
        IReadOnlyList<IInventorySlot> Slots { get; }
        int Capacity { get; }
        int UsedSlots { get; }
        event Action OnInventoryChanged;
        void NotifyChanged();
        bool AddItem(Script.Items.Item item, int amount = 1);
        void ConsumeSlot(IInventorySlot slot, int amount = 1);
        bool RemoveSlot(IInventorySlot slot);
        int CountItem(Script.Items.Item item);
        void Clear();
        IItemActionHandler ActionHandler { get; }
        void SwapSlots(int indexA, int indexB);
    }
}
