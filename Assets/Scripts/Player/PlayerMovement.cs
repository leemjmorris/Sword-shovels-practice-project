using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PathFinding;

public class PlayerMovement : MonoBehaviour, IPathfindable
{

    [Header("Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator playerAnimator;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask groundLayerMask;

    //LMJ: Pathfinding variables for IPathfindable interface
    [Header("Pathfinding")]
    [SerializeField] private PathfindingMode preferredPathfindingMode = PathfindingMode.Hybrid;
    [SerializeField] private bool requiresDynamicObstacleAvoidance = false;
    private List<Vector3> currentPath;

    //LMJ: IPathfindable interface implementation
    public Vector3 Position => transform.position;
    public float MoveSpeed => moveSpeed;
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        navMeshAgent.speed = moveSpeed;
    }

    private void Update()
    {
        //JML : Code used only in the Unity Editor
#if UNITY_EDITOR
        if (navMeshAgent.speed != moveSpeed)
        {
            navMeshAgent.speed = moveSpeed;
        }
#endif
        //LMJ: Handle mouse input for movement using PathFindManager
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            //JML : Ignore clicks on enemies
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    return;
                }
            }

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, groundLayerMask))
            {
                //LMJ: Use PathFindManager to request path
                PathFindManager.Instance.RequestPath(this, hit.point);
            }
        }

        //LMJ: Follow the path if we have one
        FollowPath();

        //LMJ: Update animator
        Vector3 horizontalVelocity = new Vector3(navMeshAgent.velocity.x, 0, navMeshAgent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        playerAnimator.SetFloat("Speed", currentSpeed);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        PathFindManager.Instance.RequestPath(this, targetPosition);
    }


    //LMJ: IPathfindable interface methods
    public void SetPath(List<Vector3> path)
    {
        currentPath = path;

        if (currentPath != null && currentPath.Count > 0)
        {
            //LMJ: Set NavMeshAgent destination to the final point
            navMeshAgent.SetDestination(currentPath[currentPath.Count - 1]);
        }
    }

    public void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        //LMJ: NavMeshAgent handles the actual movement, so we just track if we reached destination
        if (HasReachedDestination())
        {
            currentPath = null;
        }
    }

    public bool HasReachedDestination()
    {
        if (currentPath == null || currentPath.Count == 0)
            return true;

        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return true;

        //LMJ: Check if NavMeshAgent reached its destination
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //LMJ: IPathfindable interface - Get preferred pathfinding mode
    public PathfindingMode GetPreferredPathfindingMode()
    {
        return preferredPathfindingMode;
    }

    //LMJ: IPathfindable interface - Requires dynamic obstacle avoidance
    public bool RequiresDynamicObstacleAvoidance()
    {
        return requiresDynamicObstacleAvoidance;
    }

    //LMJ: Stop current movement (for interaction objects)
    public void StopMoving()
    {
        currentPath = null;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
        }
    }

    //LMJ: Called by Stomp animation event
    public void Stomp()
    {
        //LMJ: Add stomp effects here (sound, particle effects, camera shake, etc.)
    }

    //LMJ: Resume path after jump or interaction
    public void ResumePath()
    {
        if (currentPath != null && currentPath.Count > 0 && navMeshAgent.enabled)
        {
            //LMJ: Re-set destination to resume movement
            navMeshAgent.SetDestination(currentPath[currentPath.Count - 1]);
        }
    }
}
