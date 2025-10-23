using UnityEngine;

namespace PathFinding
{
    //LMJ: Component for dynamic obstacles that affect pathfinding
    public class DynamicObstacle : MonoBehaviour
    {
        [Header("Obstacle Settings")]
        [SerializeField] private bool registerOnStart = true;
        [SerializeField] private bool autoUpdateGrid = true;

        private GridManager gridManager;
        private bool isRegistered = false;

        private void Start()
        {
            gridManager = FindFirstObjectByType<GridManager>();

            if (gridManager == null)
            {
                return;
            }

            if (registerOnStart)
            {
                RegisterObstacle();
            }
        }

        private void OnEnable()
        {
            if (autoUpdateGrid && isRegistered && gridManager != null)
            {
                RegisterObstacle();
            }
        }

        private void OnDisable()
        {
            if (autoUpdateGrid && isRegistered && gridManager != null)
            {
                UnregisterObstacle();
            }
        }

        private void OnDestroy()
        {
            if (isRegistered && gridManager != null)
            {
                UnregisterObstacle();
            }
        }

        //LMJ: Manually register this obstacle to the grid
        public void RegisterObstacle()
        {
            if (gridManager == null)
            {
                return;
            }

            gridManager.RegisterDynamicObstacle(transform.position);
            isRegistered = true;

            // Trigger path recalculation for affected entities
            TriggerPathRecalculation();
        }

        //LMJ: Manually unregister this obstacle from the grid
        public void UnregisterObstacle()
        {
            if (gridManager == null)
            {
                return;
            }

            gridManager.UnregisterDynamicObstacle(transform.position);
            isRegistered = false;

            // Trigger path recalculation for affected entities
            TriggerPathRecalculation();
        }

        //LMJ: Update obstacle position (call this when moving the obstacle)
        public void UpdatePosition()
        {
            if (!isRegistered || gridManager == null)
                return;

            // Unregister from old position
            gridManager.UnregisterDynamicObstacle(transform.position);

            // Register at new position
            gridManager.RegisterDynamicObstacle(transform.position);

            TriggerPathRecalculation();
        }

        //LMJ: Trigger path recalculation for nearby pathfinding entities
        private void TriggerPathRecalculation()
        {
            //LMJ: Find all IPathfindable entities and notify them
            MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour mono in allMonoBehaviours)
            {
                IPathfindable pathfindable = mono as IPathfindable;
                if (pathfindable != null)
                {
                    //LMJ: Check if entity is close enough to be affected
                    float distance = Vector3.Distance(transform.position, pathfindable.Position);
                    if (distance < 20f) //LMJ: Recalculation radius
                    {
                        //LMJ: Entity should recalculate path (implementation depends on entity)
                    }
                }
            }
        }

        //LMJ: Draw obstacle gizmo in editor
        private void OnDrawGizmos()
        {
            Gizmos.color = isRegistered ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 20f); // Recalculation radius
        }
    }
}
