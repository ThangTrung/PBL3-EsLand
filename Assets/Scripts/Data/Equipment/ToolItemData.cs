using Core.Contracts.Equipment;
using Core.Types;
using UnityEngine;

namespace Data.Equipment
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Item/Tool")]
    public class Tool : ScriptableObject, IWeapon, IGatheringTool
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

        public float GetDamageModifier() => damage;
    }
}

