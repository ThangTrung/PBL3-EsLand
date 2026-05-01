using Core.Contracts.Inventory;
using Gameplay.Characters;
using UI.Equipment;
using UI.Inventory;
using UnityEngine;

namespace Core
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
                Debug.LogError("[GameManager] KH�NG t�m th?y Player! H�y check l?i Tag c?a Pawn_black.");
                return;
            }

            if (!playerObj.TryGetComponent<IInventoryHolder>(out var inventoryHolder))
            {
                Debug.LogError("[GameManager] Player chua implement IInventoryHolder!");
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
                Debug.Log("<color=green>[GameManager]</color> �� k?t n?i Inventory UI th�nh c�ng.");
            }

            if (equipmentUI != null)
            {
                equipmentUI.Initialize(inventoryHolder);

                if (inventoryHolder is Player player)
                {
                    player.OnToggleEquipment -= equipmentUI.ToggleUI;
                    player.OnToggleEquipment += equipmentUI.ToggleUI;
                }
                Debug.Log("<color=green>[GameManager]</color> �� k?t n?i Equipment UI th�nh c�ng.");
            }
            else
            {
                Debug.LogWarning("[GameManager] Bi?n equipmentUI dang tr?ng! H�y k�o th? v�o Inspector.");
            }
        }
    }
}



