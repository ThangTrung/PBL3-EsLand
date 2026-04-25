using Script.Entities;
using Script.Interfaces;
using UnityEngine;

namespace Script.Items
{
    public enum EquipSlot { AttackStone, HealthStone, SpeedStone, DefenseStone, MainHand}
    
    public abstract class Equipment : Item, IEquippable, IStatModifierProvider
    {
        [Header("Equipment Settings")]
        public EquipSlot equipSlot;
        public AnimatorOverrideController overrideController;
        
        public EquipSlot Slot => equipSlot;

        public virtual void OnEquip(Character character) { }
        public virtual void OnUnequip(Character character) { }

        public virtual float GetDamageModifier() => 0;
        public virtual float GetDefenseModifier() => 0;
        public virtual float GetSpeedModifier() => 0;
        public virtual float GetHealthModifier() => 0;
    }

    public abstract class DurableEquipment : Equipment, IDurable
    {
        [Header("Durability Settings")]
        public int maxDurability = 100;
        public int MaxDurability => maxDurability;
    }
}