using UnityEngine;


namespace Script.Items
{
    public enum ArmorSlot { Head, Chest, Legs, Boots }

    [CreateAssetMenu(fileName = "New Armor", menuName = "Inventory/Equipment/Armor")]
    public class Armor : Equipment 
    {
        [Header("Armor Stats")]
        [SerializeField] private float defensePower;
        [SerializeField] private ArmorSlot equipSlot;

        public float DefensePower => defensePower;
        public ArmorSlot EquipSlot => equipSlot;

        public override void OnEquip(Entities.Character user)
        {
            Debug.Log($"{user.name} đã mặc {ItemName}");
        }

        public override void OnUnequip(Entities.Character user)
        {
            Debug.Log($"{user.name} đã tháo {ItemName}");
        }
    }
}