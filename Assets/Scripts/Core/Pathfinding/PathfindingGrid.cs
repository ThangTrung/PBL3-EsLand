using System.Collections.Generic;
using UnityEngine;

namespace Core.Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        public LayerMask obstacleMask;
        public Vector2 gridWorldSize = new Vector2(30, 30);
        public float nodeRadius = 0.5f;
        [Range(0f, 1f)]
        public float collisionBuffer = 0.1f;

        [Header("Debug")]
        public bool displayGridGizmos = true;

        private PathNode[,] grid;
        private float nodeDiameter;
        private int gridSizeX, gridSizeY;

        private ContactFilter2D obstacleFilter;
        private Collider2D[] overlapResults = new Collider2D[1];

        public int MaxSize => gridSizeX * gridSizeY;

        private void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            InitializeFilter();
            CreateGrid();
        }

        private void InitializeFilter()
        {
            obstacleFilter = new ContactFilter2D();
            obstacleFilter.useTriggers = false;
            obstacleFilter.layerMask = obstacleMask;
            obstacleFilter.useLayerMask = true;
            overlapResults = new Collider2D[1];
        }

        public bool CheckSolidObstacle(Vector2 position, float radius)
        {
            if (overlapResults == null || overlapResults.Length == 0) InitializeFilter();
            return Physics2D.OverlapCircle(position, radius, obstacleFilter, overlapResults) > 0;
        }

        public void CreateGrid()
        {
            if (overlapResults == null || overlapResults.Length == 0) InitializeFilter();
            grid = new PathNode[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                    bool isObstacle = CheckSolidObstacle(worldPoint, nodeRadius + collisionBuffer);
                    grid[x, y] = new PathNode(worldPoint, x, y, isObstacle);
                }
            }

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    grid[x, y].neighbors = CalculateNeighbors(grid[x, y]);
                }
            }
        }

        private PathNode[] CalculateNeighbors(PathNode node)
        {
            List<PathNode> neighborsList = new List<PathNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighborsList.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighborsList.ToArray();
        }

        public PathNode NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = Mathf.Clamp01((worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            return grid[x, y];
        }

        public PathNode GetNodeAt(int x, int y)
        {
            if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
                return grid[x, y];
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

            if (grid != null && displayGridGizmos)
            {
                foreach (PathNode n in grid)
                {
                    Gizmos.color = n.isObstacle ? new Color(1, 0, 0, 0.5f) : new Color(1, 1, 1, 0.2f);
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
    }
}