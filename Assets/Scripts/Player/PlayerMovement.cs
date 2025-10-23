using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{

    [Header("Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator playerAnimator;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                navMeshAgent.destination = hit.point;
            }
        }

        Vector3 horizontalVelocity = new Vector3(navMeshAgent.velocity.x, 0, navMeshAgent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        playerAnimator.SetFloat("Speed", currentSpeed);
    }
}
