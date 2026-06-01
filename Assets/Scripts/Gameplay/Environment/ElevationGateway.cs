using UnityEngine;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class ElevationGateway : MonoBehaviour
    {
        [SerializeField] private string targetElevationLayer = "Elevation_A";
        private void Awake()
        {
            var col = GetComponent<PolygonCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var colLayer = collision.gameObject.layer;
            if (colLayer != 6 && colLayer != 7) return;

            var agent = collision.GetComponent<ElevationAgent>() ?? collision.GetComponentInParent<ElevationAgent>();
            
            if (agent != null)
            {
                agent.ChangeElevation(targetElevationLayer);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            var col = GetComponent<PolygonCollider2D>();
            if (col != null)
            {
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}