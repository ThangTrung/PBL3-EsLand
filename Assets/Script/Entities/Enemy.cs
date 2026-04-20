using System.Collections.Generic;
using Script.Items;
using UnityEngine;

namespace Script.Entities
{
    /// <summary>
    /// Đại diện cho kẻ thù, kế thừa từ Character với logic AI và phần thưởng khi chết.
    /// </summary>
    public class Enemy : Character
    {
        [Header("Enemy AI Settings")]
        [SerializeField] protected float detectionRange = 10f;
        [SerializeField] protected float stopDistance = 2f;
        
        [Header("Loot Settings")]
        [SerializeField] protected List<Item> lootTable = new List<Item>();
        [Range(0, 1)] [SerializeField] protected float dropChance = 0.5f;

        protected Transform target;

        protected override void Awake()
        {
            base.Awake();
            // Khởi tạo logic tìm kiếm mục tiêu (ví dụ: tìm Player)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        protected override void Update()
        {
            base.Update();
            
            if (IsDead || target == null) return;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= detectionRange)
            {
                if (distance > stopDistance)
                {
                    MoveTowardsTarget();
                }
                else
                {
                    Attack();
                }
            }
            else
            {
                Patrol();
            }
        }

        protected virtual void MoveTowardsTarget()
        {
            // Logic di chuyển cơ bản hướng về phía mục tiêu
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * (GetMoveSpeed() * Time.deltaTime);
            
            // Quay mặt về phía mục tiêu
            if (direction != Vector3.zero)
                transform.forward = direction;
        }

        public virtual void Patrol()
        {
            // TODO: Triển khai logic tuần tra (di chuyển giữa các Waypoints)
        }

        public override void Attack()
        {
            if (!CanAttack()) return;

            Debug.Log($"[Enemy] {characterName} tấn công mục tiêu! Gây {GetTotalDamage()} sát thương.");
            
            // Gây sát thương lên mục tiêu nếu mục tiêu có script Character
            if (target.TryGetComponent<Character>(out var victim))
            {
                victim.TakeDamage(GetTotalDamage());
            }

            AttackTimer = baseAttackCooldown;
        }

        protected override void Die()
        {
            base.Die();
            
            Debug.Log($"[Enemy] {characterName} đã bị tiêu diệt.");
            
            DropLoot();
            
            // Hủy object sau một khoảng thời gian hoặc chạy hiệu ứng biến mất
            Destroy(gameObject, 2f);
        }

        protected virtual void DropLoot()
        {
            if (lootTable.Count == 0) return;

            if (Random.value <= dropChance)
            {
                int randomIndex = Random.Range(0, lootTable.Count);
                Item droppedItem = lootTable[randomIndex];
                
                Debug.Log($"[Loot] {characterName} rơi ra vật phẩm: {droppedItem.ItemName}");
                
                // TODO: Instantiate Item Pickup object trong thế giới game
            }
        }
    }
}
