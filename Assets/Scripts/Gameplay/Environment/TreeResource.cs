using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Cây.
    /// Kế thừa từ ResourceNode để tận dụng logic Damageable và Interactable.
    /// </summary>
    public class TreeResource : ResourceNode
    {
        [Header("Tree Specific Settings")]
        [SerializeField] private GameObject stumpPrefab;

        private void OnEnable()
        {
            OnDie += HandleTreeFalling;
        }

        private void OnDisable()
        {
            OnDie -= HandleTreeFalling;
        }

        private void HandleTreeFalling()
        {
            if (stumpPrefab != null)
            {
                Instantiate(stumpPrefab, transform.position, transform.rotation, transform.parent);
            }
        }
    }
}
