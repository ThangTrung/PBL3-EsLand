using UnityEngine;

namespace Gameplay.World
{
    public class PlayerPickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Loot")) 
                return;
            if (other.TryGetComponent<ItemPickup>(out var item))
            {
                item.StartFlyingTowards(transform.parent != null ? transform.parent : transform);
            }
        }
    }
}

