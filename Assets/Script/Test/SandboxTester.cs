using UnityEngine;
using Script.Inventory.Controller; 
using Script.Items;    

public class InventoryTestScenario : MonoBehaviour
{
    [Header("=== GẮN PREFAB INVENTORY ===")]
    public GameObject inventoryPrefab;

    private InventoryController _spawnedController;

    void Start()
    {
        if (inventoryPrefab != null)
        {
            GameObject instance = Instantiate(inventoryPrefab);
            _spawnedController = instance.GetComponentInChildren<InventoryController>();
            
            Debug.Log("[Test] Đã load Inventory Canvas thành công!");
        }
    }

    void Update()
    {
        // Nếu chưa load xong thì không làm gì cả
        if (_spawnedController == null) return;

        // 2. BẤM PHÍM ĐỂ TEST LOGIC

        // Phím Tab: Bật/Tắt giao diện túi đồ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _spawnedController.ToggleDisplay();
        }
    }
}