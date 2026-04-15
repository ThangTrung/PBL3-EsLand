using System.Collections.Generic;
using Script.Items;
using UnityEngine;

namespace Script.World
{
    public enum ResourceType { Wood, Stone, Metal, Herb }

    public class ResourceNode : MonoBehaviour
    {
        [Header("Node Info")]
        [SerializeField] private string nodeName = "";
        [SerializeField] private ResourceType nodeType;
        [SerializeField] private float maxHealth = 50f;
        
        [Header("Gathering Requirements")]
        [SerializeField] private ToolType requiredToolType;

        [Header("Loot")]
        [SerializeField] private List<Items.Item> dropItems;

        private float _currentHealth;

        private void Start()
        {
            _currentHealth = maxHealth;
        }

        public void Harvest(Entities.Character gatherer, Tool toolUsed)
        {
            if (_currentHealth <= 0) 
                return;
            
            if (toolUsed == null || toolUsed.Type != requiredToolType)
            {
                Debug.Log($"Bạn cần một cái {requiredToolType} để khai thác {nodeName}!");
                return;
            }

            _currentHealth -= toolUsed.GatheringPower;
            Debug.Log($"Chát! {gatherer.name} đang khai thác {nodeName}. Máu còn: {_currentHealth}");

            if (_currentHealth <= 0)
                Die(gatherer);
        }

        private void Die(Entities.Character gatherer)
        {
            Debug.Log($"{nodeName} đã bị phá hủy!");
            foreach (var item in dropItems)
            {
                Debug.Log($"Đã rơi ra: {item.ItemName}");
                // Logic nhặt đồ tự động hoặc tạo ra world object ở đây
                if (gatherer.GetComponentInChildren<Inventory.InventoryController>() != null)
                {
                    gatherer.GetComponentInChildren<Inventory.InventoryController>().AddItem(item);
                }
            }
            Destroy(gameObject);
        }
    }
}