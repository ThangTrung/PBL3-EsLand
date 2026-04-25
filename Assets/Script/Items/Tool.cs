using Script.Interfaces;
using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item/Tool")]
    public class Tool : DurableEquipment, IWeapon, IGatheringTool
    {
        [Header("Tool Stats")]
        public Interfaces.ToolType toolType;
        public float damage;
        public float gatherSpeedMultiplier = 1.5f; 
        public int tier = 1;

        public float Damage => damage;
        public Interfaces.ToolType Type => toolType;
        public float GatherSpeedMultiplier => gatherSpeedMultiplier;
        public int Tier => tier;

        public override float GetDamageModifier() => damage;
    }
}