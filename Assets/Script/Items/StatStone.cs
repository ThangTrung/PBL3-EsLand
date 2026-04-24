using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Stat Stone", menuName = "Inventory/Item/Stat Stone")]
    public class StatStone : Equipment
    {
        [Header("Stone Stats")]
        public float value;

        public override float GetDamageModifier() => equipSlot == EquipSlot.PowerStone ? value : 0;
        public override float GetHealthModifier() => equipSlot == EquipSlot.HealthStone ? value : 0;
        public override float GetSpeedModifier() => equipSlot == EquipSlot.SpeedStone ? value : 0;
        public override float GetDefenseModifier() => equipSlot == EquipSlot.DefenseStone ? value : 0;
    }
}
