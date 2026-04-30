namespace Script.Inventory.Interfaces
{
    public interface IInventorySlotUI
    {
        int SlotIndex { get; }
        void Refresh(IInventorySlot slotData);
        void SetHighlight(bool active);
    }
}
