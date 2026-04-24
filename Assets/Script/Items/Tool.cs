using Script.Interfaces;
using UnityEngine;

namespace Script.Items
{
    public enum ToolType { Pickaxe, Axe, Sword, Shovel }

    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item/Tool")]
    public class Tool : DurableEquipment, IWeapon, IGatheringTool
    {
        [Header("Tool Stats")]
        public ToolType toolType;
        public float damage;
        public float gatherSpeedMultiplier = 1.5f; 
        public int tier = 1;

        public float Damage => damage;
        public ToolType ToolType => toolType;
        public float GatherSpeedMultiplier => gatherSpeedMultiplier;
        public int Tier => tier;

        public override float GetDamageModifier() => damage;
    }
}