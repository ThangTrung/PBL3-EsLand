using Core.Contracts.Inventory;
using Data.Building;
using Data.Items;
using Gameplay.Characters;
using UI.Cursor;
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
        private Collider2D ghostCollider; // Dùng Collider2D đại diện cho kích thước chiếm chỗ
        
        private Character playerCharacter; 
        private Camera _mainCamera;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Được gọi từ PlaceableItem.Use() khi bấm nút Sử dụng trong túi đồ.
        /// </summary>
        public void StartPlacement(PlaceableItem itemToPlace, Character user)
        {
            if (itemToPlace == null || itemToPlace.TargetBuilding == null)
            {
                Debug.LogError("[PlacementManager] StartPlacement failed: itemToPlace or targetBuilding is null.");
                return;
            }

            if (isPlacing) CancelPlacement();

            isPlacing = true;
            currentItemToPlace = itemToPlace;
            playerCharacter = user;

            BuildingData targetBuilding = currentItemToPlace.TargetBuilding;
            
            Debug.Log($"[PlacementManager] Đang chuẩn bị đặt: {targetBuilding.BuildingName}. Kích hoạt bóng mờ...");

            if (targetBuilding.BuildingPrefab != null)
            {
                ghostBuilding = Instantiate(targetBuilding.BuildingPrefab);
                ghostBuilding.name = "Ghost_" + targetBuilding.BuildingName;
                
                // [FIX] Tìm Collider2D bất kỳ ở Root để làm khuôn check va chạm (thay vì chỉ ép kiểu BoxCollider2D)
                ghostCollider = ghostBuilding.GetComponent<Collider2D>();
                if (ghostCollider == null)
                {
                    Debug.LogWarning($"[PlacementManager] {targetBuilding.BuildingPrefab.name} không có Collider2D ở Root. Việc check va chạm sẽ dùng bán kính mặc định (0.5).");
                }

                // 1. Tắt Collider để bóng mờ không chặn đường đi
                var colliders = ghostBuilding.GetComponentsInChildren<Collider2D>();
                foreach (var col in colliders) if (col != null) col.enabled = false;
                
                // 2. Tắt vật lý để tránh bóng mờ rơi hoặc va chạm
                var rbs = ghostBuilding.GetComponentsInChildren<Rigidbody2D>();
                foreach (var rb in rbs) if (rb != null) rb.simulated = false;

                // 3. Vô hiệu hóa logic nhưng giữ lại các script hỗ trợ hiển thị
                var scripts = ghostBuilding.GetComponentsInChildren<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    if (script == null || script == this) continue;
                    if (script is Layer.AutoAssignSortingLayer) continue;
                    script.enabled = false;
                }

                ghostRenderers = ghostBuilding.GetComponentsInChildren<SpriteRenderer>();
            }
            else
            {
                Debug.LogError($"[PlacementManager] THẤT BẠI: File BuildingData '{targetBuilding.name}' chưa được gán BuildingPrefab ở Inspector!");
                isPlacing = false;
            }
        }

        private void Update()
        {
            if (!isPlacing || ghostBuilding == null) return;

            // 2. Bóng mờ bám theo chuột
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogWarning("[PlacementManager] Main Camera not found! Cannot place building.");
                return;
            }

            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // Bắt buộc z = 0 để tránh việc bóng mờ bị khuất sau Camera 2D
            ghostBuilding.transform.position = mousePos;

            // 3. Check vị trí hợp lệ
            bool isValid = CheckPlacementValid(mousePos);
            
            // Cập nhật Cursor dựa trên tính hợp lệ của vị trí đặt
            if (CursorManager.Instance != null)
            {
                if (isValid) CursorManager.Instance.SetNormalCursor();
                else CursorManager.Instance.SetForbiddenCursor();
            }

            if (ghostRenderers != null)
            {
                foreach (var r in ghostRenderers)
                {
                    if (r != null) r.color = isValid ? validColor : invalidColor;
                }
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

            // 1. Kiểm tra địa hình phù hợp (Sử dụng OverlapCircle mượt hơn cho Tilemap)
            LayerMask targetTerrainLayer = (type == BuildingType.EscapeVehicle) ? waterLayer : landLayer;
            isOnCorrectTerrain = Physics2D.OverlapCircle(position, 0.1f, targetTerrainLayer);

            if (!isOnCorrectTerrain) return false;

            // 2. Kiểm tra không đè lên vật cản (Cây, đá, nhà khác)
            if (ghostCollider != null)
            {
                if (ghostCollider is BoxCollider2D box)
                {
                    Vector2 boxSize = box.size * 0.9f;
                    Vector2 checkPosition = position + box.offset;
                    Collider2D hit = Physics2D.OverlapBox(checkPosition, boxSize, 0f, obstacleLayer);
                    return hit == null;
                }
                else
                {
                    // Fallback dùng Bounds nếu không phải BoxCollider
                    Vector2 boundsSize = ghostCollider.bounds.size * 0.9f;
                    Collider2D hit = Physics2D.OverlapBox(position, boundsSize, 0f, obstacleLayer);
                    return hit == null;
                }
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
            
            // Tìm collider của địa hình tại vị trí đặt để lấy Parent
            Collider2D hit = Physics2D.OverlapCircle(position, 0.1f, targetLayer);
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

            // Reset cursor khi thoát chế độ xây dựng
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.SetNormalCursor();
            }
        }
    }
}
