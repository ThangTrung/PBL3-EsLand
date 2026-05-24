using System.Collections.Generic;
using Core.Contracts.Pathfinding;
using UnityEngine;

namespace Core.Pathfinding
{
    public class PathfindingTester : MonoBehaviour
    {
        public Transform seeker;
        public Transform target;
        public float seekerRadius = 0.3f; // Kích thước giả lập của nhân vật
        
        private IPathfinder pathfinder;
        private List<Vector3> currentPath;

        private void Awake()
        {
            pathfinder = GetComponent<IPathfinder>();
        }

        private void Update()
        {
            if (seeker != null && target != null && pathfinder != null)
            {
                currentPath = pathfinder.FindPath(seeker.position, target.position, seekerRadius);
            }
        }

        private void OnDrawGizmos()
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                Gizmos.color = Color.black;
                Vector3 previousPoint = currentPath[0];

                // Vẽ điểm start (ngay cả khi seeker chưa tới được currentPath[0])
                if (seeker != null)
                {
                    Gizmos.DrawLine(seeker.position, previousPoint);
                }

                foreach (Vector3 point in currentPath)
                {
                    Gizmos.DrawCube(point, Vector3.one * 0.2f);
                    Gizmos.DrawLine(previousPoint, point);
                    previousPoint = point;
                }
            }
        }
    }
}
