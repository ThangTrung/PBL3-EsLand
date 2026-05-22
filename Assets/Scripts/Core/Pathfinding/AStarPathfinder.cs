using System.Collections.Generic;
using Core.Contracts.Pathfinding;
using UnityEngine;

namespace Core.Pathfinding
{
    /// <summary>
    /// A* Pathfinding implementation optimized with Min-Heap for 2D Grid.
    /// </summary>
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

            if (IsNodeBlockedByRadius(targetNode, entityRadius))
            {
                targetNode = FindClosestWalkableNode(targetNode, entityRadius);
                if (targetNode == null) return new List<Vector3>();
            }

            Heap<PathNode> openSet = new Heap<PathNode>(grid.MaxSize);
            HashSet<PathNode> closedSet = new HashSet<PathNode>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (PathNode neighbor in currentNode.neighbors)
                {
                    if (neighbor.isObstacle || closedSet.Contains(neighbor)) continue;

                    if (entityRadius > 0f)
                    {
                        if (Physics2D.OverlapCircle(neighbor.worldPosition, entityRadius + 0.05f, grid.obstacleMask) != null)
                            continue;
                    }

                    if (!allowDiagonal && IsDiagonalMove(currentNode, neighbor)) continue;

                    if (IsDiagonalMove(currentNode, neighbor) && IsCornerCutting(currentNode, neighbor, entityRadius))
                    {
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
                        else
                            openSet.UpdateItem(neighbor);
                    }
                }
            }

            return new List<Vector3>();
        }

        private bool IsDiagonalMove(PathNode currentNode, PathNode neighbor)
        {
            return Mathf.Abs(currentNode.gridX - neighbor.gridX) == 1 && Mathf.Abs(currentNode.gridY - neighbor.gridY) == 1;
        }

        private bool IsCornerCutting(PathNode currentNode, PathNode neighbor, float entityRadius)
        {
            PathNode sideNode1 = grid.GetNodeAt(neighbor.gridX, currentNode.gridY);
            PathNode sideNode2 = grid.GetNodeAt(currentNode.gridX, neighbor.gridY);

            if (sideNode1 == null || sideNode2 == null || sideNode1.isObstacle || sideNode2.isObstacle) return true;

            if (entityRadius > 0f)
            {
                if (Physics2D.OverlapCircle(sideNode1.worldPosition, entityRadius, grid.obstacleMask) != null ||
                    Physics2D.OverlapCircle(sideNode2.worldPosition, entityRadius, grid.obstacleMask) != null)
                {
                    return true;
                }
            }

            return false;
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

            path.Reverse();
            return smoothPath ? SimplifyPath(path) : ConvertToWaypoints(path);
        }

        private List<Vector3> ConvertToWaypoints(List<PathNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            foreach (PathNode p in path) waypoints.Add(p.worldPosition);
            return waypoints;
        }

        private List<Vector3> SimplifyPath(List<PathNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            if (path.Count == 0) return waypoints;

            Vector2 directionOld = Vector2.zero;
            waypoints.Add(path[0].worldPosition);

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i].gridX - path[i - 1].gridX, path[i].gridY - path[i - 1].gridY);
                if (directionNew != directionOld)
                {
                    if (waypoints.Count == 0 || waypoints[waypoints.Count - 1] != path[i - 1].worldPosition)
                    {
                        waypoints.Add(path[i - 1].worldPosition);
                    }
                }
                directionOld = directionNew;
            }

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

            if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
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
                
                foreach (PathNode neighbor in current.neighbors)
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