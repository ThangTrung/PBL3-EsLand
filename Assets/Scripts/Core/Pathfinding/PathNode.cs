using UnityEngine;

namespace Core.Pathfinding
{
    public class PathNode
    {
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public bool isObstacle;

        // Chi phí từ điểm bắt đầu đến node này
        public int gCost;
        // Chi phí ước tính từ node này đến điểm đích (Heuristic)
        public int hCost;

        public PathNode parent;

        public PathNode(Vector3 _worldPos, int _gridX, int _gridY, bool _isObstacle)
        {
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
            isObstacle = _isObstacle;
        }

        // Tổng chi phí (fCost)
        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
    }
}
