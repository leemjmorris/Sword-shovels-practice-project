using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PathFinding
{
    //LMJ: Manages the pathfinding grid system with NavMesh integration
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 0.5f;
        [SerializeField] private bool autoDetectBounds = true;
        [SerializeField] private Vector3 manualGridOrigin = Vector3.zero;
        [SerializeField] private Vector2Int manualGridSize = new Vector2Int(100, 100);

        [Header("NavMesh Integration")]
        [SerializeField] private float navMeshSampleDistance = 0.5f;
        [SerializeField] private int navMeshAreaMask = NavMesh.AllAreas;

        [Header("Debug Visualization")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private bool showWalkableOnly = false;
        [SerializeField] private Color walkableColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color unwalkableColor = new Color(1, 0, 0, 0.3f);
        [SerializeField] private Color obstacleColor = new Color(1, 0.5f, 0, 0.5f);

        private GridNode[,] grid;
        private Vector3 gridOrigin;
        private Vector2Int gridSize;
        private Dictionary<Vector2Int, GridNode> dynamicObstacles = new Dictionary<Vector2Int, GridNode>();

        public float CellSize => cellSize;
        public Vector2Int GridSize => gridSize;
        public Vector3 GridOrigin => gridOrigin;

        private void Awake()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            if (autoDetectBounds)
            {
                DetectNavMeshBounds();
            }
            else
            {
                gridOrigin = manualGridOrigin;
                gridSize = manualGridSize;
            }

            CreateGrid();
            Debug.Log($"GridManager initialized: {gridSize.x}x{gridSize.y} grid with {cellSize}m cells");
        }

        //LMJ: Auto-detect grid bounds from NavMesh
        private void DetectNavMeshBounds()
        {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

            if (triangulation.vertices.Length == 0)
            {
                Debug.LogWarning("No NavMesh found! Using manual settings.");
                gridOrigin = manualGridOrigin;
                gridSize = manualGridSize;
                return;
            }

            // Calculate NavMesh bounds
            Vector3 min = triangulation.vertices[0];
            Vector3 max = triangulation.vertices[0];

            foreach (Vector3 vertex in triangulation.vertices)
            {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            // Add padding
            float padding = cellSize * 2;
            min -= new Vector3(padding, 0, padding);
            max += new Vector3(padding, 0, padding);

            gridOrigin = new Vector3(min.x, 0, min.z);

            int sizeX = Mathf.CeilToInt((max.x - min.x) / cellSize);
            int sizeZ = Mathf.CeilToInt((max.z - min.z) / cellSize);
            gridSize = new Vector2Int(sizeX, sizeZ);

            Debug.Log($"Auto-detected NavMesh bounds: Origin={gridOrigin}, Size={gridSize}");
        }

        //LMJ: Create grid and check walkability using NavMesh
        private void CreateGrid()
        {
            grid = new GridNode[gridSize.x, gridSize.y];

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 basePos = GetWorldPositionXZ(x, y);
                    Vector3 worldPos;
                    bool isWalkable = TryGetNavMeshPosition(basePos, out worldPos);

                    grid[x, y] = new GridNode(worldPos, new Vector2Int(x, y), isWalkable);
                }
            }
        }

        //LMJ: Get 2D world position (X, Z only) for grid cell
        private Vector3 GetWorldPositionXZ(int x, int y)
        {
            return gridOrigin + new Vector3(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f);
        }

        //LMJ: Try to get NavMesh position with correct height
        private bool TryGetNavMeshPosition(Vector3 basePosition, out Vector3 navMeshPosition)
        {
            NavMeshHit hit;

            // Sample from high above to handle elevated terrain
            Vector3 samplePos = new Vector3(basePosition.x, basePosition.y + 100f, basePosition.z);

            if (NavMesh.SamplePosition(samplePos, out hit, 105f, navMeshAreaMask))
            {
                navMeshPosition = hit.position;
                return true;
            }

            // Fallback: sample from base position
            if (NavMesh.SamplePosition(basePosition, out hit, navMeshSampleDistance, navMeshAreaMask))
            {
                navMeshPosition = hit.position;
                return true;
            }

            navMeshPosition = basePosition;
            return false;
        }

        //LMJ: Check if a position is on the NavMesh (legacy method)
        private bool IsPositionOnNavMesh(Vector3 position)
        {
            Vector3 navMeshPos;
            return TryGetNavMeshPosition(position, out navMeshPos);
        }

        //LMJ: Convert grid coordinates to world position (with NavMesh height)
        public Vector3 GetWorldPosition(int x, int y)
        {
            if (IsValidGridPosition(x, y))
            {
                return grid[x, y].WorldPosition;
            }
            return GetWorldPositionXZ(x, y);
        }

        //LMJ: Convert world position to grid coordinates
        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize);
            int y = Mathf.FloorToInt((worldPosition.z - gridOrigin.z) / cellSize);
            return new Vector2Int(x, y);
        }

        //LMJ: Get node at grid position (safe)
        public GridNode GetNode(int x, int y)
        {
            if (IsValidGridPosition(x, y))
            {
                return grid[x, y];
            }
            return null;
        }

        //LMJ: Get node at grid position (Vector2Int)
        public GridNode GetNode(Vector2Int gridPos)
        {
            return GetNode(gridPos.x, gridPos.y);
        }

        //LMJ: Get node from world position
        public GridNode GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = GetGridPosition(worldPosition);
            return GetNode(gridPos);
        }

        //LMJ: Check if grid position is valid
        public bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
        }

        public bool IsValidGridPosition(Vector2Int gridPos)
        {
            return IsValidGridPosition(gridPos.x, gridPos.y);
        }

        //LMJ: Get neighboring nodes (8 directions)
        public List<GridNode> GetNeighbors(GridNode node, bool includeDiagonals = true)
        {
            List<GridNode> neighbors = new List<GridNode>();
            Vector2Int gridPos = node.GridPosition;

            // 4 cardinal directions
            Vector2Int[] cardinalDirections = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // North
                new Vector2Int(1, 0),   // East
                new Vector2Int(0, -1),  // South
                new Vector2Int(-1, 0)   // West
            };

            // 4 diagonal directions
            Vector2Int[] diagonalDirections = new Vector2Int[]
            {
                new Vector2Int(1, 1),   // NE
                new Vector2Int(1, -1),  // SE
                new Vector2Int(-1, -1), // SW
                new Vector2Int(-1, 1)   // NW
            };

            // Add cardinal neighbors
            foreach (Vector2Int dir in cardinalDirections)
            {
                Vector2Int neighborPos = gridPos + dir;
                GridNode neighbor = GetNode(neighborPos);
                if (neighbor != null && neighbor.IsTraversable())
                {
                    neighbors.Add(neighbor);
                }
            }

            // Add diagonal neighbors if enabled
            if (includeDiagonals)
            {
                foreach (Vector2Int dir in diagonalDirections)
                {
                    Vector2Int neighborPos = gridPos + dir;
                    GridNode neighbor = GetNode(neighborPos);
                    if (neighbor != null && neighbor.IsTraversable())
                    {
                        // Check if diagonal movement is not blocked by corners
                        GridNode side1 = GetNode(gridPos.x + dir.x, gridPos.y);
                        GridNode side2 = GetNode(gridPos.x, gridPos.y + dir.y);

                        if (side1 != null && side1.IsTraversable() &&
                            side2 != null && side2.IsTraversable())
                        {
                            neighbors.Add(neighbor);
                        }
                    }
                }
            }

            return neighbors;
        }

        //LMJ: Register dynamic obstacle at world position
        public void RegisterDynamicObstacle(Vector3 worldPosition)
        {
            GridNode node = GetNodeFromWorldPosition(worldPosition);
            if (node != null)
            {
                node.HasDynamicObstacle = true;
                dynamicObstacles[node.GridPosition] = node;
                Debug.Log($"Dynamic obstacle registered at {node.GridPosition}");
            }
        }

        //LMJ: Unregister dynamic obstacle at world position
        public void UnregisterDynamicObstacle(Vector3 worldPosition)
        {
            GridNode node = GetNodeFromWorldPosition(worldPosition);
            if (node != null)
            {
                node.HasDynamicObstacle = false;
                dynamicObstacles.Remove(node.GridPosition);
                Debug.Log($"Dynamic obstacle unregistered at {node.GridPosition}");
            }
        }

        //LMJ: Reset all flood fill data in the grid
        public void ResetFloodData()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    grid[x, y].ResetFloodData();
                }
            }
        }

        //LMJ: Toggle grid visualization
        public void ToggleGridVisualization(bool show)
        {
            showGrid = show;
        }

        //LMJ: Draw grid in editor
        private void OnDrawGizmos()
        {
            if (!showGrid || grid == null) return;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    GridNode node = grid[x, y];

                    if (showWalkableOnly && !node.IsWalkable)
                        continue;

                    // Determine color
                    Color color;
                    if (node.HasDynamicObstacle)
                        color = obstacleColor;
                    else if (node.IsWalkable)
                        color = walkableColor;
                    else
                        color = unwalkableColor;

                    Gizmos.color = color;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
