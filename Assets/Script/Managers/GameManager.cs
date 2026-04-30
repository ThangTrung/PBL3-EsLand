using UnityEngine;
using Script.Entities;
using Script.Inventory.UI;
using Script.Equipment.UI;
using Script.Shared.Interfaces;

namespace Script.Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager Instance { get; set; }

        [Header("UI References")]
        [SerializeField] private InventoryPanelUI inventoryUI;
        [SerializeField] private EquipmentPanelUI equipmentUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogError("[GameManager] KHÔNG tìm thấy Player! Hãy check lại Tag của Pawn_black.");
                return;
            }

            if (!playerObj.TryGetComponent<IInventoryHolder>(out var inventoryHolder))
            {
                Debug.LogError("[GameManager] Player chưa implement IInventoryHolder!");
                return;
            }

            if (inventoryUI != null)
            {
                inventoryUI.Initialize(inventoryHolder);

                if (inventoryHolder is Player player)
                {
                    player.OnToggleInventory -= inventoryUI.ToggleUI;
                    player.OnToggleInventory += inventoryUI.ToggleUI;
                }
                Debug.Log("<color=green>[GameManager]</color> Đã kết nối Inventory UI thành công.");
            }

            if (equipmentUI != null)
            {
                equipmentUI.Initialize(inventoryHolder);

                if (inventoryHolder is Player player)
                {
                    player.OnToggleEquipment -= equipmentUI.ToggleUI;
                    player.OnToggleEquipment += equipmentUI.ToggleUI;
                }
                Debug.Log("<color=green>[GameManager]</color> Đã kết nối Equipment UI thành công.");
            }
            else
            {
                Debug.LogWarning("[GameManager] Biến equipmentUI đang trống! Hãy kéo thả vào Inspector.");
            }
        }
    }
}
