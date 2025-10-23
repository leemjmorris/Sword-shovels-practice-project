using UnityEngine;
using UnityEngine.AI;

//LMJ: DEPRECATED - This functionality is now handled by BatGoblinAI
//LMJ: BatGoblinAI uses PathFinding system instead of simple NavMesh
//LMJ: Keep this for reference or legacy enemies that don't need smart pathfinding
public class EnemyMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float stopDistance = 0.3f;
    [SerializeField] private float returnThreshold = 0.5f; 

    private Vector3 initialPosition; 
    private bool isReturning = false; 

    private void Start()
    {
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stopDistance;

        initialPosition = transform.position;
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            //JML : Player detected within range
            if (distanceToPlayer <= detectionRange)
            {
                isReturning = false;
                navMeshAgent.stoppingDistance = stopDistance;
                navMeshAgent.SetDestination(playerTransform.position);
            }
            else    //JML : Player out of range
            {

                //JML : Return to initial position
                if (!isReturning)
                {
                    isReturning = true;
                    navMeshAgent.stoppingDistance = 0f;
                    navMeshAgent.SetDestination(initialPosition);
                }

                //JML : Check if reached initial position
                if (isReturning && Vector3.Distance(transform.position, initialPosition) <= returnThreshold)
                {
                    isReturning = false;
                    navMeshAgent.ResetPath();
                }
            }
        }

        Vector3 horizontalVelocity = new Vector3(navMeshAgent.velocity.x, 0, navMeshAgent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        animator.SetFloat("Speed", currentSpeed);
    }

}
