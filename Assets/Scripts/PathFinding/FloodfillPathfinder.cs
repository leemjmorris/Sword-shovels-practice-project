using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    //LMJ: Floodfill pathfinding algorithm using BFS for dynamic obstacle avoidance
    public class FloodfillPathfinder
    {
        private GridManager gridManager;
        private float maxSearchDistance;

        public FloodfillPathfinder(GridManager gridManager, float maxSearchDistance = 15f)
        {
            this.gridManager = gridManager;
            this.maxSearchDistance = maxSearchDistance;
        }

        //LMJ: Find path using Floodfill algorithm (BFS-based)
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            GridNode startNode = gridManager.GetNodeFromWorldPosition(startPos);
            GridNode targetNode = gridManager.GetNodeFromWorldPosition(targetPos);

            if (startNode == null || targetNode == null)
            {
                Debug.LogWarning($"Floodfill: Invalid start or target position. Start={startPos}, Target={targetPos}");
                return null;
            }

            if (!startNode.IsTraversable() || !targetNode.IsTraversable())
            {
                Debug.LogWarning($"Floodfill: Start or target node is not traversable.");
                return null;
            }

            // Check if target is within search range
            float distance = Vector3.Distance(startPos, targetPos);
            if (distance > maxSearchDistance)
            {
                Debug.LogWarning($"Floodfill: Target distance {distance:F2}m exceeds max search distance {maxSearchDistance}m");
                return null;
            }

            // Reset flood data
            gridManager.ResetFloodData();

            // Perform BFS flood fill
            List<Vector3> path = BreadthFirstSearch(startNode, targetNode);

            if (path != null && path.Count > 0)
            {
                Debug.Log($"Floodfill: Found path with {path.Count} waypoints");
                return path;
            }

            Debug.LogWarning("Floodfill: Failed to find path");
            return null;
        }

        //LMJ: Breadth-First Search (BFS) implementation
        private List<Vector3> BreadthFirstSearch(GridNode startNode, GridNode targetNode)
        {
            Queue<GridNode> openQueue = new Queue<GridNode>();
            HashSet<GridNode> visitedSet = new HashSet<GridNode>();

            startNode.FloodDistance = 0;
            startNode.FloodParent = null;
            openQueue.Enqueue(startNode);
            visitedSet.Add(startNode);

            bool foundTarget = false;

            // BFS exploration
            while (openQueue.Count > 0)
            {
                GridNode currentNode = openQueue.Dequeue();

                // Check if we reached the target
                if (currentNode == targetNode)
                {
                    foundTarget = true;
                    break;
                }

                // Check distance limit (optimization)
                float distanceToStart = Vector3.Distance(currentNode.WorldPosition, startNode.WorldPosition);
                if (distanceToStart > maxSearchDistance)
                {
                    continue;
                }

                // Explore neighbors (8 directions)
                List<GridNode> neighbors = gridManager.GetNeighbors(currentNode, includeDiagonals: true);

                foreach (GridNode neighbor in neighbors)
                {
                    if (!visitedSet.Contains(neighbor))
                    {
                        neighbor.FloodDistance = currentNode.FloodDistance + 1;
                        neighbor.FloodParent = currentNode;
                        openQueue.Enqueue(neighbor);
                        visitedSet.Add(neighbor);
                    }
                }
            }

            // Reconstruct path if target found
            if (foundTarget)
            {
                return ReconstructPath(targetNode);
            }

            return null;
        }

        //LMJ: Reconstruct path from target to start using parent pointers
        private List<Vector3> ReconstructPath(GridNode targetNode)
        {
            List<Vector3> path = new List<Vector3>();
            GridNode currentNode = targetNode;

            while (currentNode != null)
            {
                path.Add(currentNode.WorldPosition);
                currentNode = currentNode.FloodParent;
            }

            path.Reverse();

            // Smooth path (remove redundant waypoints on straight lines)
            path = SmoothPath(path);

            return path;
        }

        //LMJ: Smooth path by removing redundant waypoints
        private List<Vector3> SmoothPath(List<Vector3> path)
        {
            if (path == null || path.Count <= 2)
                return path;

            List<Vector3> smoothedPath = new List<Vector3>();
            smoothedPath.Add(path[0]); // Always keep start point

            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector3 dirBefore = (path[i] - path[i - 1]).normalized;
                Vector3 dirAfter = (path[i + 1] - path[i]).normalized;

                // If direction changes significantly, keep the waypoint
                float angle = Vector3.Angle(dirBefore, dirAfter);
                if (angle > 5f) // Threshold: 5 degrees
                {
                    smoothedPath.Add(path[i]);
                }
            }

            smoothedPath.Add(path[path.Count - 1]); // Always keep end point

            return smoothedPath;
        }

        //LMJ: Refine a segment of NavMesh path using Floodfill
        public List<Vector3> RefinePath(Vector3 startPos, Vector3 endPos)
        {
            float segmentDistance = Vector3.Distance(startPos, endPos);

            // Only use Floodfill for short segments
            if (segmentDistance > maxSearchDistance)
            {
                return new List<Vector3> { startPos, endPos };
            }

            List<Vector3> refinedPath = FindPath(startPos, endPos);

            if (refinedPath != null && refinedPath.Count > 0)
            {
                return refinedPath;
            }

            // Fallback to direct line if Floodfill fails
            return new List<Vector3> { startPos, endPos };
        }

        //LMJ: Set maximum search distance
        public void SetMaxSearchDistance(float distance)
        {
            maxSearchDistance = Mathf.Max(1f, distance);
        }

        public float GetMaxSearchDistance()
        {
            return maxSearchDistance;
        }
    }
}
