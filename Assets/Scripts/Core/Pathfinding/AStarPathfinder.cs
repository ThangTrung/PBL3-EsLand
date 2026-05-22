using System.Collections.Generic;
using System.Linq;
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
        private PathfindingGrid _grid;

        public bool allowDiagonal = true;
        public bool smoothPath = true;

        private void Awake()
        {
            _grid = GetComponent<PathfindingGrid>();
        }

public List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition, float entityRadius = 0f)
{
    var originalStartNode = _grid.NodeFromWorldPoint(startPosition);
    var targetNode = _grid.NodeFromWorldPoint(targetPosition);

    if (originalStartNode == null || targetNode == null) return new List<Vector3>();

    var aStarStartNode = originalStartNode;
    List<PathNode> escapePath = null;

    if (IsNodeBlockedByRadius(originalStartNode, entityRadius))
    {
        escapePath = FindEscapePath(originalStartNode, entityRadius);
        if (escapePath == null || escapePath.Count == 0) return new List<Vector3>();
        aStarStartNode = escapePath[^1];
    }

    if (IsNodeBlockedByRadius(targetNode, entityRadius))
    {
        var targetEscape = FindEscapePath(targetNode, entityRadius);
        if (targetEscape == null || targetEscape.Count == 0) return new List<Vector3>();
        targetNode = targetEscape[^1];
    }

    var openSet = new Heap<PathNode>(_grid.MaxSize);
    var closedSet = new HashSet<PathNode>();

    openSet.Add(aStarStartNode);

    while (openSet.Count > 0)
    {
        var currentNode = openSet.RemoveFirst();
        closedSet.Add(currentNode);

        if (currentNode == targetNode)
        {
            var aStarPath = RetracePathNodes(aStarStartNode, targetNode);
            
            var fullPath = new List<PathNode>();
            
            if (escapePath != null)
            {
                fullPath.AddRange(escapePath);
            }
            else
            {
                fullPath.Add(originalStartNode);
            }

            fullPath.AddRange(aStarPath);

            var waypoints = smoothPath ? SimplifyPath(fullPath) : ConvertToWaypoints(fullPath);
            
            if (waypoints.Count > 1 && Vector3.Distance(startPosition, waypoints[0]) < 0.1f)
            {
                waypoints.RemoveAt(0);
            }

            return waypoints;
        }

        foreach (var neighbor in currentNode.neighbors)
        {
            if (neighbor.isObstacle || closedSet.Contains(neighbor)) continue;

            if (entityRadius > 0f)
            {
                if (_grid.CheckSolidObstacle(neighbor.worldPosition, entityRadius + 0.05f))
                    continue;
            }

            if (!allowDiagonal && IsDiagonalMove(currentNode, neighbor)) continue;

            if (IsDiagonalMove(currentNode, neighbor) && IsCornerCutting(currentNode, neighbor, entityRadius))
            {
                continue;
            }

            var newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
            if (newCostToNeighbor >= neighbor.gCost && openSet.Contains(neighbor)) continue;
            neighbor.gCost = newCostToNeighbor;
            neighbor.hCost = GetDistance(neighbor, targetNode);
            neighbor.parent = currentNode;

            if (!openSet.Contains(neighbor))
                openSet.Add(neighbor);
            else
                openSet.UpdateItem(neighbor);
        }
    }

    return new List<Vector3>();
}

        private static bool IsDiagonalMove(PathNode currentNode, PathNode neighbor)
        {
            return Mathf.Abs(currentNode.gridX - neighbor.gridX) == 1 && Mathf.Abs(currentNode.gridY - neighbor.gridY) == 1;
        }

        private bool IsCornerCutting(PathNode currentNode, PathNode neighbor, float entityRadius)
        {
            var sideNode1 = _grid.GetNodeAt(neighbor.gridX, currentNode.gridY);
            var sideNode2 = _grid.GetNodeAt(currentNode.gridX, neighbor.gridY);

            if (sideNode1 == null || sideNode2 == null || sideNode1.isObstacle || sideNode2.isObstacle) return true;

            if (!(entityRadius > 0f)) return false;
            return Physics2D.OverlapCircle(sideNode1.worldPosition, entityRadius, _grid.obstacleMask) ||
                   Physics2D.OverlapCircle(sideNode2.worldPosition, entityRadius, _grid.obstacleMask);
        }

private static List<PathNode> RetracePathNodes(PathNode startNode, PathNode endNode)
{
    var path = new List<PathNode>();
    var currentNode = endNode;

    while (currentNode != startNode)
    {
        path.Add(currentNode);
        currentNode = currentNode.parent;
    }

    path.Reverse();
    return path;
}

        private List<Vector3> ConvertToWaypoints(List<PathNode> path)
        {
            return path.Select(p => p.worldPosition).ToList();
        }

        private static List<Vector3> SimplifyPath(List<PathNode> path)
        {
            var waypoints = new List<Vector3>();
            if (path.Count == 0) return waypoints;

            var directionOld = Vector2.zero;
            waypoints.Add(path[0].worldPosition);

            for (var i = 1; i < path.Count; i++)
            {
                var directionNew = new Vector2(path[i].gridX - path[i - 1].gridX, path[i].gridY - path[i - 1].gridY);
                if (directionNew != directionOld)
                {
                    if (waypoints.Count == 0 || waypoints[^1] != path[i - 1].worldPosition)
                    {
                        waypoints.Add(path[i - 1].worldPosition);
                    }
                }
                directionOld = directionNew;
            }

            if (waypoints.Count == 0 || waypoints[^1] != path[^1].worldPosition)
            {
                waypoints.Add(path[^1].worldPosition);
            }

            return waypoints;
        }

        private static int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            var dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            var dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        private bool IsNodeBlockedByRadius(PathNode node, float entityRadius)
        {
            if (node.isObstacle) return true;
            if (entityRadius > 0f)
            {
                return Physics2D.OverlapCircle(node.worldPosition, entityRadius + 0.05f, _grid.obstacleMask);
            }
            return false;
        }

private List<PathNode> FindEscapePath(PathNode startSearchNode, float entityRadius)
{
    var queue = new Queue<PathNode>();
    var visited = new HashSet<PathNode>();
    var cameFrom = new Dictionary<PathNode, PathNode>();
    
    queue.Enqueue(startSearchNode);
    visited.Add(startSearchNode);

    const int maxIterations = 150;
    var currentIteration = 0;

    PathNode escapeNode = null;

    while (queue.Count > 0 && currentIteration < maxIterations)
    {
        currentIteration++;
        var current = queue.Dequeue();
        
        if (!IsNodeBlockedByRadius(current, entityRadius))
        {
            escapeNode = current;
            break;
        }
        
        foreach (var neighbor in current.neighbors)
        {
            if (!allowDiagonal && IsDiagonalMove(current, neighbor)) continue;
            if (IsDiagonalMove(current, neighbor) && IsCornerCutting(current, neighbor, entityRadius)) continue;

            if (!visited.Add(neighbor)) continue;
            cameFrom[neighbor] = current;
            queue.Enqueue(neighbor);
        }
    }

    if (escapeNode == null) return null;

    var path = new List<PathNode>();
    var curr = escapeNode;
    while (curr != startSearchNode)
    {
        path.Add(curr);
        curr = cameFrom[curr];
    }
    path.Add(startSearchNode);
    path.Reverse();
    return path;
}
    }
}