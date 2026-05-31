using UnityEngine;

namespace Layer
{
    public class AutoAssignSortingLayer : MonoBehaviour
    {
        [SerializeField]public string targetSortingLayer = "Elevation_A";
    
        public void ApplyLayer()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        
            foreach (var sr in renderers)
                sr.sortingLayerName = targetSortingLayer;
        }
    }
}