using Script.Inventory.Core;
using Script.Inventory.UI;
using Script.Items;
using UnityEngine;

namespace Script.Test
{
    public class InventoryTestScenario : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject inventoryPrefab;

        [Header("Data")]
        public Item itemToTest;
        public int amountToAdd = 1;

        private InventoryManager _spawnedManager;
        private InventoryPanelUI _inventoryUI;

        private void Start()
        {
            var instance = Instantiate(inventoryPrefab);
            _spawnedManager = instance.GetComponentInChildren<InventoryManager>();
            _inventoryUI = _spawnedManager.GetComponentInChildren<InventoryPanelUI>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                _inventoryUI.SetVisible(!_inventoryUI.IsVisible);
            if (Input.GetKeyDown(KeyCode.T))
                SimulateAddingItem();
        }

        private void SimulateAddingItem()
        {
            _spawnedManager.AddItem(itemToTest, amountToAdd);
        }
    }
}
