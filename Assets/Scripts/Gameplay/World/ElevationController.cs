using UnityEngine;

namespace Gameplay.World
{
    public enum ElevationLevel
    {
        Land = 0,
        Mountain = 1
    }

    public class ElevationController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ElevationLevel currentLevel = ElevationLevel.Land;
        [SerializeField] private int landSortingOrder = 5;
        [SerializeField] private int mountainSortingOrder = 15;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        // Physics Layers (Indices from project settings)
        private const int LandLayer = 8;
        private const int MountainLayer = 9;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                
            UpdateElevation();
        }

        public void SetElevation(ElevationLevel level)
        {
            currentLevel = level;
            UpdateElevation();
        }

        public ElevationLevel GetElevation() => currentLevel;

        private void UpdateElevation()
        {
            // Update Physics Layer
            gameObject.layer = (currentLevel == ElevationLevel.Mountain) ? MountainLayer : LandLayer;

            // Update Sorting Order
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = (currentLevel == ElevationLevel.Mountain) ? mountainSortingOrder : landSortingOrder;
                _spriteRenderer.sortingLayerName = "Default";
            }

            Debug.Log($"Player elevation updated to: {currentLevel} (Physics Layer: {LayerMask.LayerToName(gameObject.layer)}, Order: {_spriteRenderer?.sortingOrder})");
        }
    }
}
