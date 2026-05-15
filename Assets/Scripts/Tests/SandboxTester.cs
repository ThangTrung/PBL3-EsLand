using Data.Items;
using UI.Inventory;
using UnityEngine;
using Gameplay.Inventory;

namespace Tests
{
    public class InventoryTestScenario : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject inventoryPrefab;

        [Header("Data")]
        public ItemData itemToTest;
        public int amountToAdd = 1;

        private InventoryController _spawnedManager;
        private InventoryPanelUI _inventoryUI;

        private void Start()
        {
            var instance = Instantiate(inventoryPrefab);
            _spawnedManager = instance.GetComponentInChildren<InventoryController>();
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


