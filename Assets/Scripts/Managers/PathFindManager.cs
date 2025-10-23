using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PathFinding
{
    //LMJ: Hybrid NavMesh + Floodfill pathfinding manager (Singleton)
    public class PathFindManager : MonoBehaviour
    {
        public static PathFindManager Instance { get; private set; }

        [Header("Pathfinding Mode")]
        [SerializeField] private PathfindingMode defaultMode = PathfindingMode.Hybrid;

        [Header("Hybrid Settings (Option D)")]
        [SerializeField] private bool refineNavMeshSegments = true;
        [SerializeField] private float segmentRefinementDistance = 5f;
        [Tooltip("Distance threshold to switch between NavMesh and Floodfill")]
        [SerializeField] private float floodfillDistanceThreshold = 15f;

        [Header("Grid Settings")]
        [SerializeField] private GridManager gridManager;

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugPaths = true;
        [SerializeField] private Color navMeshPathColor = Color.green;
        [SerializeField] private Color floodfillPathColor = Color.blue;
        [SerializeField] private Color hybridPathColor = Color.cyan;
        [SerializeField] private float debugPathDuration = 2f;

        private NavMeshPath navMeshPath;
        private FloodfillPathfinder floodfillPathfinder;
        private List<Vector3> lastCalculatedPath;
        private PathfindingMode lastUsedMode;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            navMeshPath = new NavMeshPath();

            //LMJ: Find or create GridManager
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();

                if (gridManager == null)
                {
                    GameObject gridObj = new GameObject("GridManager");
                    gridManager = gridObj.AddComponent<GridManager>();
                }
            }

            //LMJ: Initialize Floodfill pathfinder
            floodfillPathfinder = new FloodfillPathfinder(gridManager, floodfillDistanceThreshold);

        }

        //LMJ: Find path using NavMesh only
        public List<Vector3> FindPathNavMesh(Vector3 startPos, Vector3 targetPos)
        {
            if (navMeshPath == null)
            {
                return null;
            }

            navMeshPath.ClearCorners();

            if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
            {
                if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                {
                    List<Vector3> path = new List<Vector3>(navMeshPath.corners);

                    if (showDebugPaths)
                    {
                        DrawDebugPath(path, navMeshPathColor);
                    }

                    return path;
                }
            }

            return null;
        }

        //LMJ: Find path using Floodfill only
        public List<Vector3> FindPathFloodfill(Vector3 startPos, Vector3 targetPos)
        {
            if (floodfillPathfinder == null)
            {
                return null;
            }

            List<Vector3> path = floodfillPathfinder.FindPath(startPos, targetPos);

            if (path != null && path.Count > 0 && showDebugPaths)
            {
                DrawDebugPath(path, floodfillPathColor);
            }

            return path;
        }

        //LMJ: Find path using Hybrid mode (Option D: NavMesh + Floodfill refinement)
        public List<Vector3> FindPathHybrid(Vector3 startPos, Vector3 targetPos)
        {
            // Step 1: Calculate NavMesh path for overall route
            List<Vector3> navMeshPath = FindPathNavMesh(startPos, targetPos);

            if (navMeshPath == null || navMeshPath.Count == 0)
            {
                return FindPathFloodfill(startPos, targetPos);
            }

            // Step 2: If refinement is disabled, return NavMesh path
            if (!refineNavMeshSegments)
            {
                return navMeshPath;
            }

            // Step 3: Refine each segment with Floodfill
            List<Vector3> hybridPath = new List<Vector3>();

            for (int i = 0; i < navMeshPath.Count - 1; i++)
            {
                Vector3 segmentStart = navMeshPath[i];
                Vector3 segmentEnd = navMeshPath[i + 1];
                float segmentDistance = Vector3.Distance(segmentStart, segmentEnd);

                // Add start point
                if (hybridPath.Count == 0)
                {
                    hybridPath.Add(segmentStart);
                }

                // Refine segment if within distance threshold
                if (segmentDistance <= segmentRefinementDistance)
                {
                    List<Vector3> refinedSegment = floodfillPathfinder.RefinePath(segmentStart, segmentEnd);

                    if (refinedSegment != null && refinedSegment.Count > 1)
                    {
                        // Add refined waypoints (skip first point to avoid duplicates)
                        for (int j = 1; j < refinedSegment.Count; j++)
                        {
                            hybridPath.Add(refinedSegment[j]);
                        }
                    }
                    else
                    {
                        // Fallback to direct segment
                        hybridPath.Add(segmentEnd);
                    }
                }
                else
                {
                    // Segment too long, use NavMesh waypoint directly
                    hybridPath.Add(segmentEnd);
                }
            }

            if (showDebugPaths)
            {
                DrawDebugPath(hybridPath, hybridPathColor);
            }

            return hybridPath;
        }

        //LMJ: Find path using specified mode
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, PathfindingMode mode)
        {
            lastUsedMode = mode;

            switch (mode)
            {
                case PathfindingMode.NavMeshOnly:
                    lastCalculatedPath = FindPathNavMesh(startPos, targetPos);
                    break;

                case PathfindingMode.FloodfillOnly:
                    lastCalculatedPath = FindPathFloodfill(startPos, targetPos);
                    break;

                case PathfindingMode.Hybrid:
                    lastCalculatedPath = FindPathHybrid(startPos, targetPos);
                    break;

                default:
                    lastCalculatedPath = null;
                    break;
            }

            return lastCalculatedPath;
        }

        //LMJ: Find path using default mode
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            return FindPath(startPos, targetPos, defaultMode);
        }

        //LMJ: Public API for IPathfindable entities to request paths
        public void RequestPath(IPathfindable pathfindable, Vector3 targetPosition, PathfindingMode? mode = null)
        {
            PathfindingMode selectedMode = mode ?? pathfindable.GetPreferredPathfindingMode();
            List<Vector3> path = FindPath(pathfindable.Position, targetPosition, selectedMode);

            if (path != null && path.Count > 0)
            {
                pathfindable.SetPath(path);
            }
            else
            {
            }
        }

        //LMJ: Set default pathfinding mode
        public void SetDefaultMode(PathfindingMode mode)
        {
            defaultMode = mode;
        }

        //LMJ: Toggle grid visualization
        public void ToggleGridVisualization(bool show)
        {
            if (gridManager != null)
            {
                gridManager.ToggleGridVisualization(show);
            }
        }

        //LMJ: Draw debug path in scene view
        private void DrawDebugPath(List<Vector3> path, Color color)
        {
            if (path == null || path.Count < 2)
                return;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], color, debugPathDuration);
            }

            // Draw waypoints as small spheres
            foreach (Vector3 waypoint in path)
            {
                DrawDebugSphere(waypoint, 0.2f, color);
            }
        }

        //LMJ: Draw debug sphere (approximation using lines)
        private void DrawDebugSphere(Vector3 center, float radius, Color color)
        {
            // Draw a simple cross to represent waypoint
            Debug.DrawLine(center + Vector3.up * radius, center - Vector3.up * radius, color, debugPathDuration);
            Debug.DrawLine(center + Vector3.right * radius, center - Vector3.right * radius, color, debugPathDuration);
            Debug.DrawLine(center + Vector3.forward * radius, center - Vector3.forward * radius, color, debugPathDuration);
        }

        //LMJ: Get last calculated path (for debugging)
        public List<Vector3> GetLastPath()
        {
            return lastCalculatedPath;
        }

        public PathfindingMode GetLastUsedMode()
        {
            return lastUsedMode;
        }

        public GridManager GetGridManager()
        {
            return gridManager;
        }
    }
}
