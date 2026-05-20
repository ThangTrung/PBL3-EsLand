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
        [SerializeField] private int landSortingOrder = 0;
        [SerializeField] private int mountainSortingOrder = 10;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        // Sorting Layers
        private const string LandSortingLayer = "Entities";
        private const string MountainSortingLayer = "Elevated_Entities";

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

            // Update Sorting Order and Layer
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingLayerName = (currentLevel == ElevationLevel.Mountain) ? MountainSortingLayer : LandSortingLayer;
                _spriteRenderer.sortingOrder = (currentLevel == ElevationLevel.Mountain) ? mountainSortingOrder : landSortingOrder;
            }

            Debug.Log($"Player elevation updated to: {currentLevel} (Physics Layer: {LayerMask.LayerToName(gameObject.layer)}, Sorting: {_spriteRenderer?.sortingLayerName}/{_spriteRenderer?.sortingOrder})");
        }
    }
}
