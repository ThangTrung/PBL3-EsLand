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

        public List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition, float entityRadius = 0f)
        {
            PathNode startNode = grid.NodeFromWorldPoint(startPosition);
            PathNode targetNode = grid.NodeFromWorldPoint(targetPosition);

            if (startNode == null || targetNode == null) return new List<Vector3>();

            // Nếu target bị kẹt trong vật cản (do click vào cái tháp, cái cây...), 
            // thuật toán sẽ đi tìm Node trống gần nhất xung quanh đó để đi tới.
            if (IsNodeBlockedByRadius(targetNode, entityRadius))
            {
                targetNode = FindClosestWalkableNode(targetNode, entityRadius);
                if (targetNode == null) return new List<Vector3>();
            }

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

                    // Nếu có chỉ định bán kính thực tế của nhân vật, kiểm tra xem tại điểm này có bị kẹt không
                    if (entityRadius > 0f)
                    {
                        // Kiểm tra va chạm với bán kính của nhân vật (thêm một chút xíu sai số 0.05f để tránh cạ viền)
                        if (Physics2D.OverlapCircle(neighbor.worldPosition, entityRadius + 0.05f, grid.obstacleMask) != null)
                        {
                            continue;
                        }
                    }

                    // Kiểm tra đi chéo: Nếu node lân cận là đi chéo, phải đảm bảo 2 node kề bên nó không phải vật cản
                    if (Mathf.Abs(currentNode.gridX - neighbor.gridX) == 1 && Mathf.Abs(currentNode.gridY - neighbor.gridY) == 1)
                    {
                        // Đây là node đi chéo
                        if (!allowDiagonal) continue;

                        // Check 2 node kề (ví dụ đi từ 0,0 lên 1,1 thì phải check 1,0 và 0,1)
                        PathNode sideNode1 = grid.GetNodeAt(neighbor.gridX, currentNode.gridY);
                        PathNode sideNode2 = grid.GetNodeAt(currentNode.gridX, neighbor.gridY);

                        if (sideNode1 == null || sideNode2 == null || sideNode1.isObstacle || sideNode2.isObstacle)
                        {
                            continue; // Ngăn chặn cắt góc tường
                        }
                        
                        // Cẩn thận hơn: Check collider thực tế tại 2 side node nếu nhân vật to
                        if (entityRadius > 0f)
                        {
                            if (Physics2D.OverlapCircle(sideNode1.worldPosition, entityRadius, grid.obstacleMask) != null ||
                                Physics2D.OverlapCircle(sideNode2.worldPosition, entityRadius, grid.obstacleMask) != null)
                            {
                                continue;
                            }
                        }
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
            waypoints.Add(path[0].worldPosition);

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i].gridX - path[i - 1].gridX, path[i].gridY - path[i - 1].gridY);
                if (directionNew != directionOld)
                {
                    // Khi đổi hướng, điểm bẻ góc chính là điểm path[i - 1]
                    // (Lưu ý: với i=1, directionOld = zero nên nó sẽ luôn add path[0] một lần nữa,
                    // để tránh trùng lặp ta check nếu i != 1 hoặc cứ add rồi lọc sau cũng được, 
                    // nhưng tối ưu nhất là chỉ add nếu nó khác điểm cuối đã add).
                    if (waypoints.Count == 0 || waypoints[waypoints.Count - 1] != path[i - 1].worldPosition)
                    {
                        waypoints.Add(path[i - 1].worldPosition);
                    }
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

        private bool IsNodeBlockedByRadius(PathNode node, float entityRadius)
        {
            if (node.isObstacle) return true;
            if (entityRadius > 0f)
            {
                return Physics2D.OverlapCircle(node.worldPosition, entityRadius + 0.05f, grid.obstacleMask) != null;
            }
            return false;
        }

        private PathNode FindClosestWalkableNode(PathNode startSearchNode, float entityRadius)
        {
            Queue<PathNode> queue = new Queue<PathNode>();
            HashSet<PathNode> visited = new HashSet<PathNode>();
            
            queue.Enqueue(startSearchNode);
            visited.Add(startSearchNode);

            // Giới hạn vòng lặp BFS để không tốn hiệu năng nếu cả vùng lớn bị đóng kín
            int maxIterations = 150;
            int currentIteration = 0;

            while (queue.Count > 0 && currentIteration < maxIterations)
            {
                currentIteration++;
                PathNode current = queue.Dequeue();
                
                if (!IsNodeBlockedByRadius(current, entityRadius))
                {
                    return current;
                }
                
                foreach (PathNode neighbor in grid.GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return null;
        }
    }
}
