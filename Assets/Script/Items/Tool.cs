using UnityEngine;

namespace Script.Items
{
    public enum ToolType { Sword, Bow, Pickaxe, Axe, Hoe }

    [CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Equipment/Tool")]
    public class Tool : Equipment
    {
        [Header("Tool Stats")]
        [SerializeField] private float combatDamage;
        [SerializeField] private float gatheringPower; 
        [SerializeField] private ToolType toolType;
    
        public float CombatDamage => combatDamage;
        public float GatheringPower => gatheringPower;
        public ToolType Type => toolType;

        public override void OnEquip(Entities.Character user)
        {
            Debug.Log($"{user.name} đang cầm {ItemName}");
        }

        public override void OnUnequip(Entities.Character user)
        {
            Debug.Log($"{user.name} đã bỏ {ItemName}");
        }
    }
}