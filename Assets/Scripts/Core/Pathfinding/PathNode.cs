using UnityEngine;

namespace Core.Pathfinding
{
    public class PathNode : IHeapItem<PathNode>
    {
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public bool isObstacle;

        public int gCost;
        public int hCost;
        public PathNode parent;
        public PathNode[] neighbors;
        
        private int heapIndex;

        public PathNode(Vector3 worldPos, int gridX, int gridY, bool isObstacle)
        {
            this.worldPosition = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
            this.isObstacle = isObstacle;
        }

        public int fCost => gCost + hCost;

        public int HeapIndex
        {
            get => heapIndex;
            set => heapIndex = value;
        }

        public int CompareTo(PathNode nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare; 
        }
    }
}