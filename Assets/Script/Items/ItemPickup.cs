using UnityEngine;
using Script.Inventory.Controller;

namespace Script.Items
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Dữ liệu vật phẩm")]
        public Script.Items.Item itemData;

        [Header("Cài đặt bay")]
        [SerializeField] private float flySpeed = 8f; // Tốc độ hút
        [SerializeField] private float pickupDistance = 0.2f; // Khoảng cách để biến mất

        private Transform playerTransform;
        private bool isFlying = false;

        // Hàm này sẽ được cái PickupZone của người chơi kích hoạt
        public void StartFlyingTowards(Transform target)
        {
            playerTransform = target;
            isFlying = true;
        }

        private void Update()
        {
            // Nếu đang bị hút và đã khóa mục tiêu
            if (isFlying && playerTransform != null)
            {
                // Bay dần về phía người chơi
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);

                // Nếu chạm vào người thì chui vào túi
                if (Vector2.Distance(transform.position, playerTransform.position) < pickupDistance)
                {
                    PickUp();
                }
            }
        }

        private void PickUp()
        {
            if (itemData != null)
            {
                // Gọi Singleton của Inventory và nhét data vào
                // Hàm AddItem của Khoa trả về bool, nên ta tận dụng để check túi đầy
                bool success = InventoryController.Instance.AddItem(itemData, 1);

                if (success)
                {
                    Debug.Log($"[THÀNH CÔNG] Đã nhét {itemData.ItemName} vào túi đồ.");
                    Destroy(gameObject); // Chỉ xóa khi đã nhét đồ thành công
                }
                else
                {
                    Debug.LogWarning("[THẤT BẠI] Túi đồ đã đầy, không thể nhặt thêm.");
                    isFlying = false; // Ngừng hút để đồ nằm lại đất
                }
            }
            else
            {
                Debug.LogError($"[LỖI] Vật thể {gameObject.name} chưa được gán ItemData trong Inspector!");
                Destroy(gameObject);
            }
        }
    }
}