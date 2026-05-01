using Core.Contracts.Inventory;
using Data.Items;
using UnityEngine;

namespace Gameplay.World
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Data Items")]
        public Item itemData;

        [Header("Settings")]
        [SerializeField] private float flySpeed = 5f; 
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float pickupDistance = 0.5f; 

        private Transform _playerTransform;
        private bool _isFlying;
        private float _currentSpeed;
        
        public void StartFlyingTowards(Transform target)
        {
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
                Destroy(gameObject);
            }
            else
            {
                _isFlying = false; 
            }
        }
    }
}

