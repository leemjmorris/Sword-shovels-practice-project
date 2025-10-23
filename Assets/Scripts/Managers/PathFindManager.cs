using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PathFinding
{
    //LMJ: Simple NavMesh pathfinding manager (Singleton)
    public class PathFindManager : MonoBehaviour
    {
        public static PathFindManager Instance { get; private set; }

        private NavMeshPath navMeshPath;

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
            Debug.Log("PathFindManager initialized: Using NavMesh only");
        }

        //LMJ: Find path using NavMesh
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            if (navMeshPath == null)
            {
                Debug.LogError("NavMeshPath is not initialized!");
                return null;
            }

            navMeshPath.ClearCorners();

            if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
            {
                if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                {
                    return new List<Vector3>(navMeshPath.corners);
                }
            }

            Debug.LogWarning($"Failed to find NavMesh path from {startPos} to {targetPos}");
            return null;
        }

        //LMJ: Public API for IPathfindable entities to request paths
        public void RequestPath(IPathfindable pathfindable, Vector3 targetPosition)
        {
            List<Vector3> path = FindPath(pathfindable.Position, targetPosition);

            if (path != null && path.Count > 0)
            {
                pathfindable.SetPath(path);
            }
            else
            {
                Debug.LogWarning($"Failed to find path from {pathfindable.Position} to {targetPosition}");
            }
        }
    }
}
