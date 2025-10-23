using UnityEngine;
using System.Collections;

namespace Interaction
{
    //LMJ: Jump zone that allows player to jump when pressing F key
    [RequireComponent(typeof(Collider))]
    public class JumpZone : MonoBehaviour, IInteractable
    {
        [Header("Jump Settings")]
        [SerializeField] private Transform landingPoint;
        [SerializeField] private float animationDelay = 0.5f;
        [SerializeField] private float jumpDuration = 1f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpCooldown = 0.5f;

        private bool canJump = true;
        private bool playerInZone = false;

        private void Awake()
        {
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
                PerformJump();
            }
        }

        public bool CanInteract()
        {
            return canJump && playerInZone;
        }

        public bool ShouldStopOnReach()
        {
            return true; //LMJ: Always stop at jump zones
        }

        public Vector3 GetInteractionPosition()
        {
            return transform.position;
        }

        private void PerformJump()
        {
            if (landingPoint == null)
            {
                return;
            }

            //LMJ: Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                //LMJ: Start jump sequence (animation first, then movement)
                StartCoroutine(JumpSequenceCoroutine(player));

            }
        }

        private IEnumerator JumpSequenceCoroutine(GameObject player)
        {
            canJump = false;

            Animator animator = player.GetComponent<Animator>();
            if (animator != null)
            {
                //LMJ: Trigger Stomp animation for jump
                animator.SetTrigger("Stomp");
            }

            //LMJ: Wait for animation to play
            yield return new WaitForSeconds(animationDelay);

            //LMJ: Now start the actual jump movement
            yield return StartCoroutine(JumpMovementCoroutine(player.transform));
        }

        private IEnumerator JumpMovementCoroutine(Transform playerTransform)
        {
            Vector3 startPos = playerTransform.position;
            Vector3 endPos = landingPoint.position;
            float elapsed = 0f;

            //LMJ: Disable NavMeshAgent during jump
            UnityEngine.AI.NavMeshAgent navAgent = playerTransform.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                //LMJ: Linear interpolation for horizontal movement
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

                //LMJ: Arc for vertical movement (parabola)
                float heightOffset = jumpHeight * Mathf.Sin(t * Mathf.PI);
                currentPos.y += heightOffset;

                playerTransform.position = currentPos;

                yield return null;
            }

            //LMJ: Ensure final position
            playerTransform.position = endPos;

            //LMJ: Re-enable NavMeshAgent
            if (navAgent != null)
            {
                navAgent.enabled = true;

                //LMJ: Warp NavMeshAgent to the new position
                if (navAgent.isOnNavMesh)
                {
                    navAgent.Warp(endPos);
                }
            }

            //LMJ: Resume player movement/animation
            PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ResumePath();
            }

            //LMJ: Cooldown before allowing next jump
            yield return new WaitForSeconds(jumpCooldown);
            canJump = true;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInZone = true;
                Managers.InteractionManager.Instance?.EnterInteractionZone(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInZone = false;
                Managers.InteractionManager.Instance?.ExitInteractionZone(this);
            }
        }

        private void OnDrawGizmos()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = canJump ? Color.cyan : Color.red;
                Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            }

            //LMJ: Draw line to landing point
            if (landingPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, landingPoint.position);
                Gizmos.DrawWireSphere(landingPoint.position, 0.5f);
            }
        }
    }
}
