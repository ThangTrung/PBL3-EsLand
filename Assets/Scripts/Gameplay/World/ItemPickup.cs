using Core.Contracts.Inventory;
using Data.Items;
using UnityEngine;
using System.Linq;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;
using Infrastructure.Pooling;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;

namespace Gameplay.World
{
    [RequireComponent(typeof(SaveableEntity))]
    public class ItemPickup : MonoBehaviour, ISaveable 
    {
        [Header("Data Items")]
        public ItemData itemData;
        public int quantity = 1;

        [Header("Settings")]
        [SerializeField] private float flySpeed = 5f; 
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float pickupDistance = 0.5f; 
        [SerializeField] private float pickupDelay = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioData pickupSound;

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

        private void OnEnable()
        {
            _spawnTime = Time.time;
            _isFlying = false;
            _playerTransform = null;

            // [SENIOR FIX] Chỉ sinh ID mới nếu ID hiện tại đang trống (Dành cho vật phẩm rơi từ quái/cây)
            // Vật phẩm đặt sẵn trong Scene sẽ giữ nguyên ID cố định để Save/Load hoạt động.
            if (TryGetComponent<SaveableEntity>(out var saveable))
            {
                if (string.IsNullOrEmpty(saveable.Id))
                {
                    saveable.ForceNewId();
                }
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

        public void SetItem(ItemData data, int qty)
        {
            itemData = data;
            quantity = qty;
        }

        private void PickUp()
        {
            if (!itemData || !_playerTransform) return;
            if (!_playerTransform.TryGetComponent<IInventoryHolder>(out var holder) || holder.Inventory == null)
                return;
        
            var success = holder.Inventory.AddItem(itemData, quantity);
            if (success)
            {
                // Phát âm thanh nhặt đồ
                if (AudioManager.Instance != null && pickupSound != null)
                {
                    AudioManager.Instance.PlaySFX(pickupSound, transform.position);
                }

                // 🔥 ĐÚNG CHUẨN SRP: Chỉ báo cáo trạng thái vào RAM, giao phó việc lưu ổ cứng cho SaveLoadManager
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.RegisterDestroyedEntity(_uniqueInstanceID);
                }

                ObjectPoolManager.Instance.ReturnToPool(gameObject);
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
                ObjectPoolManager.Instance.ReturnToPool(gameObject);
            }
        }

        public void SaveData(GameData data)
        {
            // Trống
        }
    }
}