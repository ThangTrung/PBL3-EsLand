using Core.Contracts.Inventory;
using Data.Items;
using UnityEngine;
using System.Linq;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;

namespace Gameplay.World
{
    [RequireComponent(typeof(SaveableEntity))]
    public class ItemPickup : MonoBehaviour, ISaveable 
    {
        [Header("Data Items")]
        public ItemData itemData;

        [Header("Settings")]
        [SerializeField] private float flySpeed = 5f; 
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float pickupDistance = 0.5f; 
        [SerializeField] private float pickupDelay = 0.5f;

        private Transform _playerTransform;
        private bool _isFlying;
        private float _currentSpeed;
        private float _spawnTime;
        
        private string _uniqueInstanceID; 
        
        private void Awake()
        {
            _spawnTime = Time.time;
            
            if (TryGetComponent<SaveableEntity>(out var saveable))
            {
                _uniqueInstanceID = saveable.Id;
            }
        }

        public void StartFlyingTowards(Transform target)
        {
            if (Time.time < _spawnTime + pickupDelay) return;
            
            _playerTransform = target;
            _isFlying = true;
            _currentSpeed = flySpeed;
        }

        private void Update()
        {
            if (!_isFlying || !_playerTransform) 
                return;
            _currentSpeed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _currentSpeed * Time.deltaTime);
            
            if (Vector2.Distance(transform.position, _playerTransform.position) < pickupDistance)
                PickUp();
        }

        private void PickUp()
        {
            if (!itemData || !_playerTransform) return;
            if (!_playerTransform.TryGetComponent<IInventoryHolder>(out var holder) || holder.Inventory == null)
                return;
        
            var success = holder.Inventory.AddItem(itemData, 1);
            if (success)
            {
                // 🔥 ĐÚNG CHUẨN SRP: Chỉ báo cáo trạng thái vào RAM, giao phó việc lưu ổ cứng cho SaveLoadManager
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.RegisterDestroyedEntity(_uniqueInstanceID);
                }

                Destroy(gameObject);
            }
            else
            {
                _isFlying = false; 
            }
        }

        public void LoadData(GameData data)
        {
            if (data != null && data.destroyedEntityIDs != null && data.destroyedEntityIDs.Contains(_uniqueInstanceID))
            {
                Destroy(gameObject);
            }
        }

        public void SaveData(GameData data)
        {
            // Trống
        }
    }
}