using UnityEngine;

namespace Script.Items
{
    [CreateAssetMenu(fileName = "New Armor", menuName = "Inventory/Item/Armor")]
    public class Armor : Equipment
    {
        [Header("Armor Stats")]
        public float defense;
        public float movementSpeedPenalty = 0f;
    }
}