using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PathFinding;

//LMJ: Monster spawner that pre-spawns monsters and activates them when player is near
//LMJ: Place on WayPoints in the map
//LMJ: Uses GridManager to ensure monsters spawn on walkable ground only
public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int maxSpawnAttempts = 10;

    [Header("Activation Settings")]
    [SerializeField] private float activationRange = 15f;
    [SerializeField] private bool activateOnce = true;

    [Header("Spawn Positions")]
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private bool useRandomPositions = true;

    [Header("Ground Validation")]
    [SerializeField] private bool validateSpawnPosition = true;
    [SerializeField] private float groundCheckHeight = 10f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Grid-Based Spawn (Recommended)")]
    [SerializeField] private bool useGridBasedSpawn = true;
    [Tooltip("Search for walkable grid nodes within this radius")]
    [SerializeField] private float gridSearchRadius = 5f;
    [Tooltip("Maximum number of grid cells to check when searching for spawn position")]
    [SerializeField] private int maxGridSearchAttempts = 50;

    private List<GameObject> spawnedMonsters = new List<GameObject>();
    private Transform player;
    private bool hasActivated = false;
    private GridManager gridManager;

    private void Start()
    {
        //LMJ: Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        //LMJ: Find GridManager for spawn position validation
        gridManager = FindFirstObjectByType<GridManager>();

        //LMJ: Pre-spawn monsters (inactive)
        PreSpawnMonsters();
    }

    private void Update()
    {
        //LMJ: Check if player is in activation range
        if (player != null && !hasActivated)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= activationRange)
            {
                ActivateMonsters();

                if (activateOnce)
                {
                    hasActivated = true;
                }
            }
        }
    }

    private void PreSpawnMonsters()
    {
        if (monsterPrefab == null)
            return;

        //LMJ: Clear existing spawned monsters
        foreach (GameObject monster in spawnedMonsters)
        {
            if (monster != null)
                Destroy(monster);
        }
        spawnedMonsters.Clear();

        //LMJ: Spawn monsters at designated positions
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, transform);

            //LMJ: Set monster name
            monster.name = $"{monsterPrefab.name}_{i}";

            //LMJ: Deactivate monster initially
            monster.SetActive(false);

            spawnedMonsters.Add(monster);
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        //LMJ: Use predefined spawn points if available
        if (spawnPoints != null && spawnPoints.Count > 0 && index < spawnPoints.Count)
        {
            if (spawnPoints[index] != null)
            {
                Vector3 position = spawnPoints[index].position;

                //LMJ: Validate spawn point using Grid if enabled
                if (validateSpawnPosition && useGridBasedSpawn && gridManager != null)
                {
                    //LMJ: Check if spawn point is on walkable grid
                    if (IsPositionWalkable(position))
                    {
                        GridNode node = gridManager.GetNodeFromWorldPosition(position);
                        if (node != null && node.IsTraversable())
                        {
                            return node.WorldPosition; // Use grid-corrected position
                        }
                    }

                    //LMJ: If not walkable, try to find nearby walkable node
                    Vector3 nearbyWalkable = FindNearestWalkablePosition(position, gridSearchRadius);
                    if (nearbyWalkable != Vector3.zero)
                    {
                        Debug.LogWarning($"MonsterSpawner: Spawn point {index} is not walkable, using nearby position");
                        return nearbyWalkable;
                    }
                }

                return validateSpawnPosition ? GetValidGroundPosition(position) : position;
            }
        }

        //LMJ: Otherwise, use random position around spawner
        if (useRandomPositions)
        {
            return GetRandomValidPosition();
        }

        //LMJ: Default: spawner position
        return validateSpawnPosition ? GetValidGroundPosition(transform.position) : transform.position;
    }

    private Vector3 GetRandomValidPosition()
    {
        //LMJ: Prefer grid-based spawning if available and enabled
        if (useGridBasedSpawn && gridManager != null)
        {
            Vector3 gridBasedPosition = GetRandomWalkablePositionFromGrid();
            if (gridBasedPosition != Vector3.zero)
            {
                return gridBasedPosition;
            }
            Debug.LogWarning($"MonsterSpawner: Grid-based spawn failed, falling back to NavMesh validation");
        }

        //LMJ: Fallback: Try multiple times to find a valid spawn position using NavMesh
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (!validateSpawnPosition)
                return randomPosition;

            //LMJ: Check if position is on walkable ground
            Vector3 validPosition = GetValidGroundPosition(randomPosition);

            //LMJ: Verify the position is actually walkable using GridManager
            if (IsPositionWalkable(validPosition))
            {
                return validPosition;
            }
        }

        //LMJ: Fallback to spawner position if no valid position found
        return GetValidGroundPosition(transform.position);
    }

    //LMJ: Get random walkable position using GridManager (most reliable method)
    private Vector3 GetRandomWalkablePositionFromGrid()
    {
        return GetRandomWalkablePositionFromGrid(transform.position, gridSearchRadius);
    }

    //LMJ: Get random walkable position near a target position
    private Vector3 GetRandomWalkablePositionFromGrid(Vector3 centerPosition, float searchRadius)
    {
        if (gridManager == null)
            return Vector3.zero;

        //LMJ: Get grid position of center
        Vector2Int centerGridPos = gridManager.GetGridPosition(centerPosition);

        //LMJ: Calculate search radius in grid cells
        int searchRadiusCells = Mathf.CeilToInt(searchRadius / gridManager.CellSize);

        //LMJ: Collect all walkable nodes within radius
        List<GridNode> walkableNodes = new List<GridNode>();

        for (int xOffset = -searchRadiusCells; xOffset <= searchRadiusCells; xOffset++)
        {
            for (int yOffset = -searchRadiusCells; yOffset <= searchRadiusCells; yOffset++)
            {
                //LMJ: Check if within circular radius (not square)
                float distanceInCells = Mathf.Sqrt(xOffset * xOffset + yOffset * yOffset);
                if (distanceInCells > searchRadiusCells)
                    continue;

                Vector2Int checkPos = centerGridPos + new Vector2Int(xOffset, yOffset);
                GridNode node = gridManager.GetNode(checkPos);

                if (node != null && node.IsWalkable && node.IsTraversable())
                {
                    walkableNodes.Add(node);
                }

                //LMJ: Stop if we've checked too many cells (performance limit)
                if (walkableNodes.Count + (xOffset * yOffset) > maxGridSearchAttempts)
                    break;
            }
        }

        //LMJ: Return random walkable position
        if (walkableNodes.Count > 0)
        {
            GridNode randomNode = walkableNodes[Random.Range(0, walkableNodes.Count)];
            return randomNode.WorldPosition;
        }

        //LMJ: No walkable nodes found
        return Vector3.zero;
    }

    //LMJ: Find nearest walkable position to a target using Grid (BFS-like spiral search)
    private Vector3 FindNearestWalkablePosition(Vector3 targetPosition, float maxSearchRadius)
    {
        if (gridManager == null)
            return Vector3.zero;

        //LMJ: Get grid position
        Vector2Int targetGridPos = gridManager.GetGridPosition(targetPosition);

        //LMJ: Check center position first
        GridNode centerNode = gridManager.GetNode(targetGridPos);
        if (centerNode != null && centerNode.IsWalkable && centerNode.IsTraversable())
        {
            return centerNode.WorldPosition;
        }

        //LMJ: Spiral outward to find nearest walkable node
        int maxRadiusCells = Mathf.CeilToInt(maxSearchRadius / gridManager.CellSize);

        for (int radius = 1; radius <= maxRadiusCells; radius++)
        {
            for (int xOffset = -radius; xOffset <= radius; xOffset++)
            {
                for (int yOffset = -radius; yOffset <= radius; yOffset++)
                {
                    //LMJ: Only check the "ring" at current radius
                    if (Mathf.Abs(xOffset) != radius && Mathf.Abs(yOffset) != radius)
                        continue;

                    Vector2Int checkPos = targetGridPos + new Vector2Int(xOffset, yOffset);
                    GridNode node = gridManager.GetNode(checkPos);

                    if (node != null && node.IsWalkable && node.IsTraversable())
                    {
                        return node.WorldPosition;
                    }
                }
            }
        }

        return Vector3.zero;
    }

    private Vector3 GetValidGroundPosition(Vector3 position)
    {
        //LMJ: First, try to sample NavMesh directly
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(position, out navHit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            //LMJ: Found a valid NavMesh position nearby
            return navHit.position;
        }

        //LMJ: Fallback: Raycast down to find ground
        RaycastHit hit;
        Vector3 rayStart = new Vector3(position.x, position.y + groundCheckHeight, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckHeight * 2f))
        {
            //LMJ: Try to sample NavMesh at raycast hit point
            if (NavMesh.SamplePosition(hit.point, out navHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                return navHit.position;
            }

            return hit.point;
        }

        //LMJ: No ground found, return original position (may fail to spawn)
        Debug.LogWarning($"MonsterSpawner: Could not find valid spawn position near {position}");
        return position;
    }

    private bool IsPositionWalkable(Vector3 worldPosition)
    {
        //LMJ: Primary check: Is position on NavMesh?
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(worldPosition, out navHit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            //LMJ: Position is on NavMesh - check distance to sampled point
            float distance = Vector3.Distance(worldPosition, navHit.position);
            if (distance <= navMeshSampleDistance)
            {
                return true;
            }
        }

        //LMJ: Fallback: Check with GridManager if available
        if (gridManager != null)
        {
            GridNode node = gridManager.GetNodeFromWorldPosition(worldPosition);
            return node != null && node.IsWalkable;
        }

        //LMJ: If no validation methods available, assume not walkable (safe default)
        return false;
    }

    private void ActivateMonsters()
    {
        foreach (GameObject monster in spawnedMonsters)
        {
            if (monster != null)
            {
                monster.SetActive(true);

                //LMJ: Set initial position for MonsterAI to return to
                MonsterAI monsterAI = monster.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    //LMJ: Initial position is already set in MonsterAI.Awake()
                }
            }
        }
    }

    //LMJ: Public method to manually respawn monsters (if needed)
    public void RespawnMonsters()
    {
        hasActivated = false;
        PreSpawnMonsters();
    }

    //LMJ: Check how many monsters are still alive
    public int GetAliveMonsterCount()
    {
        int count = 0;
        foreach (GameObject monster in spawnedMonsters)
        {
            if (monster != null && monster.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    private void OnDrawGizmos()
    {
        //LMJ: Draw activation range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        //LMJ: Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        //LMJ: Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        //LMJ: Draw spawn preview positions
        if (useRandomPositions)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 previewPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                Gizmos.DrawWireSphere(previewPosition, 0.3f);
            }
        }
    }
}
