using UnityEngine;

namespace Script.Items
{
    public enum ToolType { Pickaxe, Axe, Sword, Shovel }

    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item/Tool")]
    public class Tool : Equipment
    {
        [Header("Tool Stats")]
        public ToolType toolType;
        public float damage;
        public float gatherSpeedMultiplier = 1.5f; 
        public int tier = 1; 

        // private void Awake()
        // {
        //     maxStackSize = 1;
        //     equipSlot = EquipSlot.MainHand; // Công cụ luôn cầm ở tay chính
        // }
    }
}