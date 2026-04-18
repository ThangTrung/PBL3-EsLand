using Script.Inventory.Controller;
using Script.Inventory.UI;
using Script.Items;
using UnityEngine;
//using Script.Data.Items; 

namespace Script.Test
{
    public class InventoryTestScenario : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject inventoryPrefab;
        
        [Header("Data")]
        public Item itemToTest; 
        public int amountToAdd = 1;

        private InventoryController _spawnedController;
        private InventoryUI _inventoryUI;

        private void Start()
        {
            var instance = Instantiate(inventoryPrefab);
            _spawnedController = instance.GetComponentInChildren<InventoryController>();
            _inventoryUI = _spawnedController.GetComponentInChildren<InventoryUI>();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
                _inventoryUI.SetVisible(!_inventoryUI.IsVisible);
            if (Input.GetKeyDown(KeyCode.T))
            {
                SimulateAddingItem();
            }
        }

        private void SimulateAddingItem()
        {
            _spawnedController.AddItem(itemToTest, amountToAdd);
        }
    }
}