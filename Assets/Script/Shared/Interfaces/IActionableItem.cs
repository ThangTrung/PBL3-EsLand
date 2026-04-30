namespace Script.Shared.Interfaces
{
    public interface IActionableItem
    {
        string DisplayName { get; }
        bool CanUse { get; }
        bool CanDrop { get; }
        bool CanEquip { get; }
        bool CanUnequip { get; }
        void Use();
        void Drop();
        void Equip();
        void Unequip();
    }
}
