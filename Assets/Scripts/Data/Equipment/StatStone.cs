using UnityEngine;

namespace Data.Equipment
{
    [CreateAssetMenu(fileName = "New Power Stone", menuName = "Inventory/ItemData/Power Stone")]
    public class StatStoneItemData : Equipment
    {
        [Header("Stone Buffs")]
        [SerializeField] private float damageBuff;
        [SerializeField] private float defenseBuff;
        [SerializeField] private float healthBuff;
        [SerializeField] private float speedBuff;

        public override float GetDamageModifier() => damageBuff;
        public override float GetDefenseModifier() => defenseBuff;
        public override float GetHealthModifier() => healthBuff;
        public override float GetSpeedModifier() => speedBuff;
    }
}
