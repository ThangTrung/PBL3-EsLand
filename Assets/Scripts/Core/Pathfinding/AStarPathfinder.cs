using System.Collections.Generic;
using Core.Contracts.Pathfinding;
using UnityEngine;

namespace Core.Pathfinding
{
    [RequireComponent(typeof(PathfindingGrid))]
    public class AStarPathfinder : MonoBehaviour, IPathfinder
    {
        private PathfindingGrid grid;

        public bool allowDiagonal = true;
        public bool smoothPath = true;

        private void Awake()
        {
            grid = GetComponent<PathfindingGrid>();
        }

        public List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition)
        {
            PathNode startNode = grid.NodeFromWorldPoint(startPosition);
            PathNode targetNode = grid.NodeFromWorldPoint(targetPosition);

            if (startNode == null || targetNode == null) return new List<Vector3>();

            // Nếu target không thể đến được, tìm node gần nhất có thể đến (tùy chọn)
            // Hiện tại ta cứ bỏ qua nếu target bị kẹt trong tường.
            // Nhưng để linh hoạt, có thể cho phép target trong tường và lấy điểm gần nhất bên ngoài.
            // Đoạn này ta giả định target phải nằm vùng walk-able (hoặc ít nhất mình đi đến sát vùng đó).

            List<PathNode> openSet = new List<PathNode>();
            HashSet<PathNode> closedSet = new HashSet<PathNode>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (PathNode neighbor in grid.GetNeighbors(currentNode))
                {
                    if (neighbor.isObstacle || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // Không cho đi chéo nếu bị cản ở 2 bên góc
                    if (!allowDiagonal)
                    {
                        if (Mathf.Abs(currentNode.gridX - neighbor.gridX) == 1 && Mathf.Abs(currentNode.gridY - neighbor.gridY) == 1)
                            continue;
                    }

                    int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector3>(); // Không tìm thấy đường
        }

        private List<Vector3> RetracePath(PathNode startNode, PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            PathNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            // Path đang từ cuối lên đầu, cần đảo ngược lại
            path.Reverse();

            if (smoothPath)
            {
                return SimplifyPath(path);
            }
            else
            {
                List<Vector3> waypoints = new List<Vector3>();
                foreach (PathNode p in path)
                {
                    waypoints.Add(p.worldPosition);
                }
                return waypoints;
            }
        }

        private List<Vector3> SimplifyPath(List<PathNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            if (path.Count == 0) return waypoints;

            Vector2 directionOld = Vector2.zero;
            
            // Luôn thêm điểm đầu tiên của chuỗi path vào.
            // path[0] có thể coi là node đầu tiên sau startNode.
            waypoints.Add(path[0].worldPosition);

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i].gridX - path[i - 1].gridX, path[i].gridY - path[i - 1].gridY);
                if (directionNew != directionOld)
                {
                    // Đổi hướng => ghi lại node TRƯỚC điểm đổi hướng (để quẹo).
                    // Tuy nhiên, logic chuẩn của A* là nếu directionNew đổi, node hiện tại (path[i]) là node ta phải tới để bắt đầu quẹo.
                    waypoints.Add(path[i].worldPosition);
                }
                directionOld = directionNew;
            }

            // Đảm bảo điểm đích cuối cùng luôn có
            if (waypoints.Count == 0 || waypoints[waypoints.Count - 1] != path[path.Count - 1].worldPosition)
            {
                waypoints.Add(path[path.Count - 1].worldPosition);
            }

            return waypoints;
        }

        private int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
