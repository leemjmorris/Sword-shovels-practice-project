using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Interaction
{
    //LMJ: Three-part door system that opens once and cannot be closed
    [RequireComponent(typeof(Collider))]
    public class InteractableDoor : MonoBehaviour, IInteractable
    {
        [Header("Door Parts")]
        [SerializeField] private GameObject leftDoor;
        [SerializeField] private GameObject rightDoor;
        [SerializeField] private GameObject topDoor;

        [Header("Movement Settings")]
        [SerializeField] private Vector3 leftDoorMovement = new Vector3(-2f, 0f, 0f);
        [SerializeField] private Vector3 rightDoorMovement = new Vector3(2f, 0f, 0f);
        [SerializeField] private Vector3 topDoorMovement = new Vector3(0f, 3f, 0f);

        [Header("Animation Settings")]
        [SerializeField] private float openDuration = 2f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Initial State")]
        [SerializeField] private bool startClosed = true;

        [Header("NavMesh Obstacle Settings")]
        [SerializeField] private bool autoManageNavMeshObstacles = true;
        [Tooltip("Manually assigned obstacle objects (optional). If provided, these will be used instead of auto-creating obstacles on door parts.")]
        [SerializeField] private GameObject[] manualObstacles;

        private bool isOpened = false;
        private bool isOpening = false;

        private Vector3 leftDoorStartPos;
        private Vector3 rightDoorStartPos;
        private Vector3 topDoorStartPos;

        private NavMeshObstacle[] doorObstacles;

        private void Awake()
        {
            //LMJ: Store initial positions (use world position for prefab objects)
            //LMJ: These are the OPEN positions (how doors are positioned in editor for NavMesh baking)
            if (leftDoor != null)
                leftDoorStartPos = leftDoor.transform.position;
            if (rightDoor != null)
                rightDoorStartPos = rightDoor.transform.position;
            if (topDoor != null)
                topDoorStartPos = topDoor.transform.position;

            //LMJ: Ensure trigger is set
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            //LMJ: Setup NavMeshObstacles if auto-management is enabled
            if (autoManageNavMeshObstacles)
            {
                SetupNavMeshObstacles();
            }
        }

        private void Start()
        {
            //LMJ: Close doors at game start if needed (for NavMesh baking)
            if (startClosed)
            {
                CloseDoorImmediate();
            }
        }

        private void CloseDoorImmediate()
        {
            //LMJ: Move doors to closed position instantly (reverse of movement)
            if (leftDoor != null)
                leftDoor.transform.position = leftDoorStartPos - leftDoorMovement;
            if (rightDoor != null)
                rightDoor.transform.position = rightDoorStartPos - rightDoorMovement;
            if (topDoor != null)
                topDoor.transform.position = topDoorStartPos - topDoorMovement;
        }

        public void Interact()
        {
            if (CanInteract())
            {
                StartCoroutine(OpenDoorCoroutine());
            }
        }

        public bool CanInteract()
        {
            return !isOpened && !isOpening;
        }

        public bool ShouldStopOnReach()
        {
            return !isOpened; //LMJ: Only stop if door is not opened yet
        }

        public Vector3 GetInteractionPosition()
        {
            return transform.position;
        }

        private IEnumerator OpenDoorCoroutine()
        {
            isOpening = true;

            float elapsed = 0f;

            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = openCurve.Evaluate(elapsed / openDuration);

                if (leftDoor != null)
                {
                    Vector3 newPos = leftDoorStartPos + leftDoorMovement * t;
                    leftDoor.transform.position = newPos;

                    //LMJ: Handle Rigidbody if exists
                    Rigidbody rb = leftDoor.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.MovePosition(newPos);
                    }
                }

                if (rightDoor != null)
                {
                    Vector3 newPos = rightDoorStartPos + rightDoorMovement * t;
                    rightDoor.transform.position = newPos;

                    Rigidbody rb = rightDoor.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.MovePosition(newPos);
                    }
                }

                if (topDoor != null)
                {
                    Vector3 newPos = topDoorStartPos + topDoorMovement * t;
                    topDoor.transform.position = newPos;

                    Rigidbody rb = topDoor.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.MovePosition(newPos);
                    }
                }

                yield return null;
            }

            //LMJ: Ensure final positions are exact
            if (leftDoor != null)
                leftDoor.transform.position = leftDoorStartPos + leftDoorMovement;
            if (rightDoor != null)
                rightDoor.transform.position = rightDoorStartPos + rightDoorMovement;
            if (topDoor != null)
                topDoor.transform.position = topDoorStartPos + topDoorMovement;

            isOpening = false;
            isOpened = true;

            //LMJ: Disable NavMeshObstacles when door is fully open
            if (autoManageNavMeshObstacles)
            {
                SetNavMeshObstaclesEnabled(false);
            }
        }

        //LMJ: Setup NavMeshObstacles on each door part or use manual obstacles
        private void SetupNavMeshObstacles()
        {
            System.Collections.Generic.List<NavMeshObstacle> obstacles = new System.Collections.Generic.List<NavMeshObstacle>();

            //LMJ: If manual obstacles are provided, use them
            if (manualObstacles != null && manualObstacles.Length > 0)
            {
                foreach (GameObject obstacleObj in manualObstacles)
                {
                    if (obstacleObj != null)
                    {
                        NavMeshObstacle obstacle = obstacleObj.GetComponent<NavMeshObstacle>();
                        if (obstacle != null)
                        {
                            //LMJ: Configure the obstacle
                            obstacle.carving = true;
                            obstacle.carveOnlyStationary = false;
                            obstacles.Add(obstacle);
                        }
                        else
                        {
                            Debug.LogWarning($"Manual obstacle '{obstacleObj.name}' does not have a NavMeshObstacle component!");
                        }
                    }
                }
            }
            else
            {
                //LMJ: Auto-create obstacles on door parts (legacy behavior)
                if (leftDoor != null)
                {
                    NavMeshObstacle obstacle = SetupObstacleOnDoor(leftDoor);
                    if (obstacle != null) obstacles.Add(obstacle);
                }

                if (rightDoor != null)
                {
                    NavMeshObstacle obstacle = SetupObstacleOnDoor(rightDoor);
                    if (obstacle != null) obstacles.Add(obstacle);
                }

                if (topDoor != null)
                {
                    NavMeshObstacle obstacle = SetupObstacleOnDoor(topDoor);
                    if (obstacle != null) obstacles.Add(obstacle);
                }
            }

            doorObstacles = obstacles.ToArray();
        }

        //LMJ: Setup NavMeshObstacle on a single door GameObject (auto-create mode)
        private NavMeshObstacle SetupObstacleOnDoor(GameObject doorObject)
        {
            NavMeshObstacle obstacle = doorObject.GetComponent<NavMeshObstacle>();

            if (obstacle == null)
            {
                obstacle = doorObject.AddComponent<NavMeshObstacle>();
            }

            //LMJ: Configure the obstacle
            obstacle.carving = true; // Enable carving to cut holes in NavMesh
            obstacle.carveOnlyStationary = false; // Allow carving while moving (during door open animation)
            obstacle.shape = NavMeshObstacleShape.Box;

            //LMJ: Try to match the collider size
            Collider doorCollider = doorObject.GetComponent<Collider>();
            if (doorCollider != null)
            {
                obstacle.center = doorCollider.bounds.center - doorObject.transform.position;
                obstacle.size = doorCollider.bounds.size;
            }
            else
            {
                //LMJ: Default size if no collider found
                obstacle.center = Vector3.zero;
                obstacle.size = new Vector3(1f, 2f, 0.2f);
            }

            return obstacle;
        }

        //LMJ: Enable or disable all door obstacles
        private void SetNavMeshObstaclesEnabled(bool enabled)
        {
            if (doorObstacles == null) return;

            foreach (NavMeshObstacle obstacle in doorObstacles)
            {
                if (obstacle != null)
                {
                    obstacle.enabled = enabled;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Managers.InteractionManager.Instance?.EnterInteractionZone(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Managers.InteractionManager.Instance?.ExitInteractionZone(this);
            }
        }

        private void OnDrawGizmos()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = isOpened ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            }

            //LMJ: Draw NavMeshObstacle preview (only in editor)
            if (autoManageNavMeshObstacles && !Application.isPlaying)
            {
                DrawObstaclePreview();
            }
        }

        //LMJ: Draw obstacle size preview for manual or auto obstacles
        private void DrawObstaclePreview()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange

            //LMJ: If manual obstacles are provided, draw them
            if (manualObstacles != null && manualObstacles.Length > 0)
            {
                foreach (GameObject obstacleObj in manualObstacles)
                {
                    if (obstacleObj != null)
                    {
                        NavMeshObstacle obstacle = obstacleObj.GetComponent<NavMeshObstacle>();
                        if (obstacle != null)
                        {
                            Vector3 center = obstacleObj.transform.position + obstacleObj.transform.TransformVector(obstacle.center);
                            Vector3 size = Vector3.Scale(obstacle.size, obstacleObj.transform.lossyScale);
                            Gizmos.DrawWireCube(center, size);
                        }
                    }
                }
            }
            else
            {
                //LMJ: Draw auto-created obstacles preview
                if (leftDoor != null)
                {
                    DrawSingleObstaclePreview(leftDoor);
                }

                if (rightDoor != null)
                {
                    DrawSingleObstaclePreview(rightDoor);
                }

                if (topDoor != null)
                {
                    DrawSingleObstaclePreview(topDoor);
                }
            }
        }

        //LMJ: Draw single obstacle preview for auto-created obstacles
        private void DrawSingleObstaclePreview(GameObject doorObject)
        {
            Collider doorCollider = doorObject.GetComponent<Collider>();
            if (doorCollider != null)
            {
                Vector3 obstacleSize = doorCollider.bounds.size;
                Vector3 obstacleCenter = doorCollider.bounds.center;
                Gizmos.DrawWireCube(obstacleCenter, obstacleSize);
            }
            else
            {
                Vector3 obstacleSize = new Vector3(1f, 2f, 0.2f);
                Vector3 obstacleCenter = doorObject.transform.position;
                Gizmos.DrawWireCube(obstacleCenter, obstacleSize);
            }
        }
    }
}
