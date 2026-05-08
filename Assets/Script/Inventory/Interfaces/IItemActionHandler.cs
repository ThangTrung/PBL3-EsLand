namespace Script.Inventory.Interfaces
{
    public interface IItemActionHandler
    {
        void UseItem(IInventorySlot slot);
        void DropItem(IInventorySlot slot);
        void EquipItem(IInventorySlot slot);
        void UnequipItem(Script.Items.EquipSlot slot);
        bool IsEquipped(IInventorySlot slot);
    }
}
