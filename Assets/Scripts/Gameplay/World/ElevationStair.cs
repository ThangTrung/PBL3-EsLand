using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElevationStair : MonoBehaviour
    {
        [Header("Target Elevation")]
        [SerializeField] private ElevationLevel targetLevel;

        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent<ElevationController>(out var controller))
                {
                    controller.SetElevation(targetLevel);
                }
            }
        }
    }
}
