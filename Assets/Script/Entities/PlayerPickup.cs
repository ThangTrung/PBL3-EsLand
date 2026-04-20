using Script.Items; // Để nhận diện được file ItemPickup
using UnityEngine;

namespace Script.Entities
{
    public class PlayerPickup : MonoBehaviour
    {
        // Khi có vật thể lọt vào vòng tròn (CircleCollider2D)
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Quét xem vật đó có gắn Tag "Loot" giống ảnh của Khoa không
            if (other.CompareTag("Loot"))
            {
                // Lấy bộ não ItemPickup của cục đồ đó
                ItemPickup item = other.GetComponent<ItemPickup>();

                if (item != null)
                {
                    // Ra lệnh cho nó bay về phía nhân vật chính
                    // Lấy transform.parent vì đoạn code này đang nằm ở thằng con (PickupZone)
                    item.StartFlyingTowards(transform.parent);
                }
            }
        }
    }
}