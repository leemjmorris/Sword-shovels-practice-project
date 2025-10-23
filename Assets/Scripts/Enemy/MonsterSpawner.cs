using System.Collections.Generic;
using UnityEngine;
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
        //LMJ: Try multiple times to find a valid spawn position
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

    private Vector3 GetValidGroundPosition(Vector3 position)
    {
        //LMJ: Raycast down to find ground
        RaycastHit hit;
        Vector3 rayStart = new Vector3(position.x, position.y + groundCheckHeight, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckHeight * 2f))
        {
            return hit.point;
        }

        //LMJ: No ground found, return original position
        return position;
    }

    private bool IsPositionWalkable(Vector3 worldPosition)
    {
        //LMJ: If no GridManager, assume position is valid
        if (gridManager == null)
            return true;

        //LMJ: Check if position is on a walkable node in the grid
        GridNode node = gridManager.GetNodeFromWorldPosition(worldPosition);

        return node != null && node.isWalkable;
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
