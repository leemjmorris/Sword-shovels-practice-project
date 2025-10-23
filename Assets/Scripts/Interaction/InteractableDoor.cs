using UnityEngine;
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

        private bool isOpened = false;
        private bool isOpening = false;

        private Vector3 leftDoorStartPos;
        private Vector3 rightDoorStartPos;
        private Vector3 topDoorStartPos;

        private void Awake()
        {
            //LMJ: Store initial positions (use world position for prefab objects)
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
        }
    }
}
