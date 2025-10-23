using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    //LMJ: Base class for pathfinding entities, inherit this for Player/Monster
    [RequireComponent(typeof(CharacterController))]
    public class PathfindableEntity : MonoBehaviour, IPathfindable
    {
        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float rotationSpeed = 10f;
        [SerializeField] protected float stoppingDistance = 0.5f;

        [Header("Path Settings")]
        [SerializeField] protected bool autoFollowPath = true;
        [SerializeField] protected bool drawPathGizmos = true;

        [Header("Pathfinding Mode")]
        [SerializeField] protected PathfindingMode preferredPathfindingMode = PathfindingMode.Hybrid;
        [SerializeField] protected bool requiresDynamicObstacleAvoidance = false;

        protected CharacterController characterController;
        protected List<Vector3> currentPath;
        protected int currentWaypointIndex = 0;
        protected bool isFollowingPath = false;

        public Vector3 Position => transform.position;
        public float MoveSpeed => moveSpeed;

        protected virtual void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        protected virtual void Update()
        {
            if (autoFollowPath && isFollowingPath)
            {
                FollowPath();
            }
        }

        public virtual void SetPath(List<Vector3> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning($"{gameObject.name}: Received empty path!");
                return;
            }

            currentPath = path;
            currentWaypointIndex = 0;
            isFollowingPath = true;

            Debug.Log($"{gameObject.name}: Path set with {path.Count} waypoints");
        }

        public virtual void FollowPath()
        {
            if (!isFollowingPath || currentPath == null || currentPath.Count == 0)
                return;

            if (currentWaypointIndex >= currentPath.Count)
            {
                OnReachedDestination();
                return;
            }

            Vector3 targetWaypoint = currentPath[currentWaypointIndex];

            //LMJ: Check if we should stop at an interaction object
            if (CheckForInteractionObject(targetWaypoint))
            {
                return;
            }

            Vector3 direction = (targetWaypoint - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            if (characterController.enabled)
            {
                movement.y = -9.81f * Time.deltaTime;
                characterController.Move(movement);
            }
            else
            {
                transform.position += movement;
            }

            float distanceToWaypoint = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                        new Vector3(targetWaypoint.x, 0, targetWaypoint.z));

            if (distanceToWaypoint < stoppingDistance)
            {
                currentWaypointIndex++;
            }
        }

        public virtual bool HasReachedDestination()
        {
            if (currentPath == null || currentPath.Count == 0)
                return true;

            return currentWaypointIndex >= currentPath.Count;
        }

        protected virtual void OnReachedDestination()
        {
            isFollowingPath = false;
            currentPath = null;
            currentWaypointIndex = 0;
            Debug.Log($"{gameObject.name}: Reached destination!");
        }

        //LMJ: Request path to target position from PathFindManager
        public virtual void MoveTo(Vector3 targetPosition)
        {
            if (PathFindManager.Instance != null)
            {
                PathFindManager.Instance.RequestPath(this, targetPosition);
            }
            else
            {
                Debug.LogError("PathFindManager not found in scene!");
            }
        }

        public virtual void StopMoving()
        {
            isFollowingPath = false;
            currentPath = null;
            currentWaypointIndex = 0;
        }

        //LMJ: IPathfindable interface implementation - Get preferred pathfinding mode
        public virtual PathfindingMode GetPreferredPathfindingMode()
        {
            return preferredPathfindingMode;
        }

        //LMJ: IPathfindable interface implementation - Requires dynamic obstacle avoidance
        public virtual bool RequiresDynamicObstacleAvoidance()
        {
            return requiresDynamicObstacleAvoidance;
        }

        //LMJ: Set preferred pathfinding mode at runtime
        public virtual void SetPreferredPathfindingMode(PathfindingMode mode)
        {
            preferredPathfindingMode = mode;
        }

        //LMJ: Check if there's an interaction object nearby that requires stopping
        protected virtual bool CheckForInteractionObject(Vector3 targetPosition)
        {
            var interactionManager = FindFirstObjectByType<global::Managers.InteractionManager>();
            if (interactionManager == null)
                return false;

            global::Interaction.IInteractable currentInteractable = interactionManager.GetCurrentInteractable();

            if (currentInteractable != null && currentInteractable.ShouldStopOnReach())
            {
                float distanceToInteraction = Vector3.Distance(transform.position, currentInteractable.GetInteractionPosition());

                if (distanceToInteraction < stoppingDistance * 2f) // Stop a bit earlier for interaction
                {
                    StopMoving();
                    Debug.Log($"{gameObject.name}: Stopped at interaction object");
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnDrawGizmos()
        {
            if (!drawPathGizmos || currentPath == null || currentPath.Count == 0)
                return;

            Gizmos.color = Color.green;

            for (int i = 0; i < currentPath.Count; i++)
            {
                Vector3 waypoint = currentPath[i];
                Gizmos.DrawWireSphere(waypoint, 0.3f);

                if (i > 0)
                {
                    Gizmos.DrawLine(currentPath[i - 1], waypoint);
                }
            }

            if (currentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentPath[currentWaypointIndex], 0.5f);
            }
        }
    }
}
