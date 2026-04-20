using UnityEngine;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    [Header("Cài đặt tài nguyên")]
    [SerializeField] private float staminaCostPerHit = 5f;
    public float GetStaminaCost() => staminaCostPerHit;
    [SerializeField] private string resourceName;
    [SerializeField] private int health = 3; // Số lần đập thì vỡ
    [SerializeField] private GameObject pickupPrefab; 

    [Header("Danh sách đồ rơi (Loot Table)")]
    [SerializeField] private List<LootItem> lootTable;

    [System.Serializable]
    public class LootItem
    {
        public Script.Items.Item item; // File ScriptableObject của món đồ
        public int minAmount = 1;
        public int maxAmount = 3;
    }

    // Hàm này sẽ được Player gọi khi vung cuốc/rìu
    public void GetHit(int damage)
    {
        health -= damage;

        // Hiệu ứng rung rinh khi bị đập (Option)
        transform.localScale = Vector3.one * 1.2f;

        if (health <= 0)
        {
            SpawnLoot();
            Destroy(gameObject); // Vỡ mỏ quặng
        }
    }

    private void Update()
    {
        // Hồi phục lại scale sau khi bị đập
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 10f);
    }

    private void SpawnLoot()
    {
        foreach (var loot in lootTable)
        {
            int count = Random.Range(loot.minAmount, loot.maxAmount + 1);
            for (int i = 0; i < count; i++)
            {
                // 1. Tạo ra "xác" cục đồ
                GameObject droppedObj = Instantiate(pickupPrefab, transform.position, Quaternion.identity);

                // 2. "Thổi hồn" (Data) vào cục đồ đó
                var pickupScript = droppedObj.GetComponent<Script.Items.ItemPickup>();
                if (pickupScript != null)
                {
                    // Quan trọng: Gán đúng loại Item cho cục đồ vừa rơi ra
                    // Khoa cần sửa lại biến itemData trong ItemPickup thành 'public' hoặc tạo thêm hàm Init nhé!
                    pickupScript.itemData = loot.item;
                }

                // 3. Làm cho đồ văng ra một chút cho đẹp (Lực ném ngẫu nhiên)
                Rigidbody2D rb = droppedObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    float force = Random.Range(5f, 7f);
                    rb.AddForce(randomDir * force, ForceMode2D.Impulse);
                }
            }
        }
    }
}