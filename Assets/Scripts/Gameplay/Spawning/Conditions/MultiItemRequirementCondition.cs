using UnityEngine;
using Core.Contracts.Spawning;
using Core.Contracts.Inventory;
using Data.Items;
using System.Collections.Generic;

namespace Gameplay.Spawning.Conditions
{
    public class MultiItemRequirementCondition : MonoBehaviour, ISpawnCondition
    {
        [System.Serializable]
        public struct ItemRequirement
        {
            public ItemData item;
            public int amount;
        }

        [Tooltip("Danh sách các vật phẩm yêu cầu để mở khóa (Ví dụ: 4 viên đá sức mạnh khác nhau)")]
        [SerializeField] private List<ItemRequirement> requiredItems = new List<ItemRequirement>();

        [Tooltip("Câu thông báo nếu không đủ điều kiện")]
        [SerializeField] private string feedbackMessage = "Bạn chưa thu thập đủ 4 Viên Đá Sức Mạnh từ các Boss khác!";

        public bool IsMet(Transform player)
        {
            if (requiredItems == null || requiredItems.Count == 0) return true;
            if (player == null) return false;

            var inventoryHolder = player.GetComponent<IInventoryHolder>();
            if (inventoryHolder == null || inventoryHolder.Inventory == null) return false;

            foreach (var req in requiredItems)
            {
                if (req.item != null && inventoryHolder.Inventory.CountItem(req.item) < req.amount)
                {
                    return false;
                }
            }

            return true;
        }

        public string GetFeedbackMessage()
        {
            return feedbackMessage;
        }
    }
}