namespace Script.Inventory.Interfaces
{
    public interface IInventoryUI
    {
        bool IsVisible { get; }
        void SetVisible(bool visible);
        void RefreshUI();
        void SelectSlot(int index);
    }
}
