using UnityEngine;

namespace PathFinding
{
    //LMJ: Represents a single node in the pathfinding grid
    public class GridNode
    {
        public Vector3 WorldPosition { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsWalkable { get; set; }
        public bool HasDynamicObstacle { get; set; }

        // Floodfill algorithm data
        public int FloodDistance { get; set; }
        public GridNode FloodParent { get; set; }
        public bool Visited { get; set; }

        public GridNode(Vector3 worldPosition, Vector2Int gridPosition, bool isWalkable)
        {
            WorldPosition = worldPosition;
            GridPosition = gridPosition;
            IsWalkable = isWalkable;
            HasDynamicObstacle = false;
            ResetFloodData();
        }

        public void ResetFloodData()
        {
            FloodDistance = int.MaxValue;
            FloodParent = null;
            Visited = false;
        }

        public bool IsTraversable()
        {
            return IsWalkable && !HasDynamicObstacle;
        }

        public override string ToString()
        {
            return $"GridNode[{GridPosition.x},{GridPosition.y}] World:{WorldPosition} Walkable:{IsWalkable} Obstacle:{HasDynamicObstacle}";
        }
    }
}
