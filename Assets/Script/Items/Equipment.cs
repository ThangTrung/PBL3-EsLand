using UnityEngine;

namespace Script.Items
{
    public enum EquipSlot { Head, Chest, Legs, Feet, MainHand, OffHand }
    public abstract class Equipment : Item 
    {
        [Header("Equipment Settings")]
        public EquipSlot equipSlot;
        public int maxDurability = 100;
        
        public int MaxDurability => maxDurability;
        
        
        // Hàm Reset thông số khi mới tạo ra
        // private void Awake()
        // {
        //     maxStackSize = 1; // Trang bị không được cộng dồn (Stack)
        // }

        // public override void Use()
        // {
        //     // Logic khi bấm Use (hoặc chuột phải) vào trang bị -> Mặc vào người
        //     Debug.Log($"Đang trang bị {itemName} vào vị trí {equipSlot}");
        //     // TODO: Gọi hệ thống Equipment System để mặc đồ
        // }
    }
}