using UnityEngine;

namespace Data.Loot
{
    [System.Serializable]
    public struct LootDropData
    {
        public string ItemId;
        public int Quantity;
        public Vector3 DropPosition;

        public LootDropData(string itemId, int quantity, Vector3 dropPosition)
        {
            ItemId = itemId;
            Quantity = quantity;
            DropPosition = dropPosition;
        }
    }
}