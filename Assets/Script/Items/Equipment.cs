using UnityEngine;

namespace Script.Items
{
    public abstract class Equipment : Item 
    {
        [Header("Equipment Settings")]
        [SerializeField] [Min(1)] protected int maxDurability = 100;

        private int _currentDurability;

        protected virtual void OnEnable()
        {
            _currentDurability = maxDurability;
        }

        public int MaxDurability => maxDurability;
        public int Durability => _currentDurability;

        public override bool Use(Entities.Character user)
        {
            return user.Equip(this);
        }

        public abstract void OnEquip(Entities.Character user);
        public abstract void OnUnequip(Entities.Character user);
    }
}