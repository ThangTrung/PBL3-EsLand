using Script.Inventory.Controller;
using UnityEngine;

namespace Script.Entities
{
    /// <summary>
    /// Đại diện cho người chơi, kế thừa từ Character với các cơ chế sinh tồn riêng.
    /// </summary>
    public class Player : Character
    {
        [Header("Survival Stats")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;

        private float _hunger;
        private float _thirst;
        private float _stamina;

        public float Hunger { get => _hunger; private set => _hunger = value; }
        public float Thirst { get => _thirst; private set => _thirst = value; }
        public float Stamina { get => _stamina; private set => _stamina = value; }

        protected override void Awake()
        {
            base.Awake();
            Hunger = maxHunger;
            Thirst = maxThirst;
            Stamina = maxStamina;
            
            // Tự động tìm Inventory nếu chưa gán
            if (inventory == null)
                inventory = GetComponentInChildren<InventoryController>(true);
        }

        protected override void Update()
        {
            base.Update();
            HandleInventoryInput();
            
            // Giảm dần các chỉ số sinh tồn theo thời gian (ví dụ đơn giản)
            ReduceSurvivalStats(Time.deltaTime * 0.5f);
        }

        public void ToggleInventory()
        {
            if (inventory == null) return;
            var ui = inventory.GetComponent<Script.Inventory.UI.InventoryUI>();
            if (ui != null)
            {
                ui.SetVisible(!ui.IsVisible);
            }
        }

        private void HandleInventoryInput()
        {
            if (Input.GetKeyDown(KeyCode.E))
                ToggleInventory();
        }

        private void ReduceSurvivalStats(float amount)
        {
            Hunger = Mathf.Max(0, Hunger - amount);
            Thirst = Mathf.Max(0, Thirst - amount);
            
            // Nếu đói hoặc khát quá mức, sẽ bị mất máu
            if (Hunger <= 0 || Thirst <= 0)
            {
                TakeDamage(Time.deltaTime * 2f);
            }
        }

        public void Consume(float hungerAmount, float thirstAmount, float healthAmount)
        {
            Hunger = Mathf.Min(Hunger + hungerAmount, maxHunger);
            Thirst = Mathf.Min(Thirst + thirstAmount, maxThirst);
            Heal(healthAmount);
            Debug.Log($"[Player] Đã hồi phục: Đói +{hungerAmount}, Khát +{thirstAmount}, Máu +{healthAmount}");
        }

        public override void Attack()
        {
            if (!CanAttack()) return;

            Debug.Log($"[Player] Tấn công! Gây {GetTotalDamage()} sát thương.");
            
            // Reset cooldown đòn đánh
            AttackTimer = baseAttackCooldown;
            
            // TODO: Triển khai logic gây sát thương thực tế (Raycast, Trigger, v.v.)
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log("[Player] GAME OVER! Người chơi đã hy sinh.");
            // Triển khai logic Game Over hoặc Respawn ở đây
        }
    }
}