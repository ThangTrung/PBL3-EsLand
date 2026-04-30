using Script.Equipment.Interfaces;
using UnityEngine;
using ToolType = Script.Equipment.Interfaces.ToolType;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item/Tool")]
    public class Tool : DurableEquipment, IWeapon, IGatheringTool
    {
        [Header("Tool Stats")]
        public ToolType toolType; 
        public float damage;
        public float gatherSpeedMultiplier = 1.5f;
        public int tier = 1;

        public float Damage => damage;
        public ToolType Type => toolType;
        public float GatherSpeedMultiplier => gatherSpeedMultiplier;
        public int Tier => tier;

        public override float GetDamageModifier() => damage;
    }
}