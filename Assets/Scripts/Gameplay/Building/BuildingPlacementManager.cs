using Core.Contracts.Inventory;
using Data.Building;
using Data.Items;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Building
{
    public class BuildingPlacementManager : MonoBehaviour
    {
        public static BuildingPlacementManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Layer của vật cản (cây, đá, quái, công trình khác) để check va chạm khi đặt nhà")]
        [SerializeField] private LayerMask obstacleLayer; 
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private LayerMask landLayer;
        [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

        private bool isPlacing = false;
        public bool IsPlacing => isPlacing;

        private PlaceableItem currentItemToPlace; // Bản vẽ item đang được cầm
        private GameObject ghostBuilding; // Bóng mờ
        private SpriteRenderer[] ghostRenderers;
        private BoxCollider2D ghostCollider; // Dùng BoxCollider2D đại diện cho kích thước chiếm chỗ
        
        private Character playerCharacter; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Được gọi từ PlaceableItem.Use() khi bấm nút Sử dụng trong túi đồ.
        /// </summary>
        public void StartPlacement(PlaceableItem itemToPlace, Character user)
        {
            if (isPlacing) CancelPlacement();

            isPlacing = true;
            currentItemToPlace = itemToPlace;
            playerCharacter = user;

            BuildingData targetBuilding = currentItemToPlace.TargetBuilding;
            
            Debug.Log($"[PlacementManager] Đang chuẩn bị đặt: {targetBuilding.BuildingName}. Kích hoạt bóng mờ...");

            if (targetBuilding.BuildingPrefab != null)
            {
                ghostBuilding = Instantiate(targetBuilding.BuildingPrefab);
                
                // Lấy ra BoxCollider2D gốc để làm khuôn check va chạm
                ghostCollider = ghostBuilding.GetComponent<BoxCollider2D>();
                if (ghostCollider == null)
                {
                    Debug.LogWarning($"[PlacementManager] {targetBuilding.BuildingPrefab.name} không có BoxCollider2D ở Root. Việc check va chạm sẽ dùng kích thước mặc định (1x1).");
                }

                // Tắt Collider để bóng mờ không chặn đường đi
                var colliders = ghostBuilding.GetComponentsInChildren<Collider2D>();
                foreach (var col in colliders) col.enabled = false;
                
                // Tắt các script logic (VD: CookingTower) trên bóng mờ
                var scripts = ghostBuilding.GetComponentsInChildren<MonoBehaviour>();
                foreach (var script in scripts) Destroy(script);

                ghostRenderers = ghostBuilding.GetComponentsInChildren<SpriteRenderer>();
            }
            else
            {
                Debug.LogError($"[PlacementManager] THẤT BẠI: File BuildingData '{targetBuilding.name}' chưa được gán BuildingPrefab ở Inspector!");
            }
        }

        private void Update()
        {
            if (!isPlacing || ghostBuilding == null) return;

            // 2. Bóng mờ bám theo chuột
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // Bắt buộc z = 0 để tránh việc bóng mờ bị khuất sau Camera 2D
            ghostBuilding.transform.position = mousePos;

            // 3. Check vị trí hợp lệ
            bool isValid = CheckPlacementValid(mousePos);
            foreach (var r in ghostRenderers)
            {
                r.color = isValid ? validColor : invalidColor;
            }

            // 4. Đặt công trình
            if (Input.GetMouseButtonDown(0) && isValid)
            {
                PlaceBuilding(mousePos);
            }

            // 5. Hủy đặt (Chuột phải)
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }

        private bool CheckPlacementValid(Vector2 position)
        {
            if (currentItemToPlace == null || currentItemToPlace.TargetBuilding == null) return false;

            BuildingType type = currentItemToPlace.TargetBuilding.Type;
            bool isOnCorrectTerrain = false;

            // 1. Kiểm tra địa hình phù hợp (Water cho Thuyền, Land cho công trình khác)
            if (type == BuildingType.EscapeVehicle)
            {
                // Thuyền: Phải chạm vào lớp Nước
                isOnCorrectTerrain = Physics2D.OverlapPoint(position, waterLayer);
            }
            else
            {
                // Công trình khác: Phải chạm vào lớp Đất
                isOnCorrectTerrain = Physics2D.OverlapPoint(position, landLayer);
            }

            if (!isOnCorrectTerrain) return false;

            // 2. Kiểm tra không đè lên vật cản (Cây, đá, nhà khác)
            if (ghostCollider != null)
            {
                Vector2 boxSize = ghostCollider.size * 0.9f;
                Vector2 checkPosition = position + ghostCollider.offset;
                Collider2D hit = Physics2D.OverlapBox(checkPosition, boxSize, 0f, obstacleLayer);
                return hit == null;
            }
            else
            {
                Collider2D hit = Physics2D.OverlapCircle(position, 0.45f, obstacleLayer);
                return hit == null;
            }
        }

        private void PlaceBuilding(Vector2 position)
        {
            // [FIX] Tìm Parent (Elevation Layer) phù hợp tại vị trí đặt
            Transform parent = null;
            BuildingType type = currentItemToPlace.TargetBuilding.Type;
            LayerMask targetLayer = (type == BuildingType.EscapeVehicle) ? waterLayer : landLayer;
            
            Collider2D hit = Physics2D.OverlapPoint(position, targetLayer);
            if (hit != null)
            {
                // Thường Tilemap sẽ là con của Elevation_A/B/C
                parent = hit.transform.parent;
            }

            // A. Đặt công trình thật
            Instantiate(currentItemToPlace.TargetBuilding.BuildingPrefab, position, Quaternion.identity, parent);

            // B. Trừ Item bản vẽ khỏi túi đồ người chơi
            var inventoryHolder = playerCharacter.GetComponentInChildren<IInventoryHolder>();
            if (inventoryHolder != null && inventoryHolder.Inventory != null)
            {
                inventoryHolder.Inventory.RemoveItem(currentItemToPlace, 1);
            }

            // C. Dọn dẹp
            CancelPlacement();
        }

        private void CancelPlacement()
        {
            isPlacing = false;
            currentItemToPlace = null;
            ghostCollider = null;
            if (ghostBuilding != null)
            {
                Destroy(ghostBuilding);
            }
        }
    }
}
