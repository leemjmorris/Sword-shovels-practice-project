using UnityEngine;
using PathFinding;

//LMJ: Generic Monster AI that chases the player using PathFinding system
//LMJ: Combines Enemy stats/combat with PathFinding for smart navigation
//LMJ: Can be used for all monster types (BatGoblin, Goblin, Orc, etc.)
public class MonsterAI : PathfindableEntity
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private float returnThreshold = 0.5f;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Combat")]
    [SerializeField] private Enemy enemyStats;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Death Zone")]
    [SerializeField] private float deathZoneY = -10f;

    private Vector3 initialPosition;
    private float lastPathUpdateTime = 0f;
    private float lastAttackTime = 0f;
    private bool isChasing = false;
    private bool isReturning = false;
    private bool isInAttackRange = false;

    protected override void Awake()
    {
        base.Awake();

        //LMJ: Store initial position for returning
        initialPosition = transform.position;

        //LMJ: Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        //LMJ: Get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyStats == null)
            enemyStats = GetComponent<Enemy>();
    }

    protected override void Update()
    {
        base.Update();

        //LMJ: Check if monster fell below death zone
        if (transform.position.y < deathZoneY)
        {
            HandleFallDeath();
            return;
        }

        if (player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //LMJ: Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            isReturning = false;

            //LMJ: If in attack range, stop and attack
            if (distanceToPlayer <= attackRange)
            {
                StopMoving();
                isInAttackRange = true;
                FacePlayer();
                TryAttack();
            }
            else
            {
                isInAttackRange = false;

                //LMJ: Chase player - update path periodically
                if (Time.time - lastPathUpdateTime >= pathUpdateInterval)
                {
                    ChasePlayer();
                    lastPathUpdateTime = Time.time;
                }

                isChasing = true;
            }
        }
        else
        {
            //LMJ: Player out of range
            isInAttackRange = false;

            if (isChasing)
            {
                //LMJ: Return to initial position
                isChasing = false;
                ReturnToInitialPosition();
            }

            //LMJ: Check if reached initial position
            if (isReturning)
            {
                float distanceToInitial = Vector3.Distance(transform.position, initialPosition);
                if (distanceToInitial <= returnThreshold)
                {
                    isReturning = false;
                    StopMoving();
                }
            }
        }

        //LMJ: Update animator
        UpdateAnimator();
    }

    private void ChasePlayer()
    {
        //LMJ: Use existing PathFinding system to chase player
        MoveTo(player.position);
    }

    private void ReturnToInitialPosition()
    {
        //LMJ: Use PathFinding to return to initial position
        isReturning = true;
        MoveTo(initialPosition);
    }

    private void FacePlayer()
    {
        //LMJ: Face the player during attack
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void TryAttack()
    {
        //LMJ: Attack with cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        //LMJ: Trigger attack through Enemy stats system
        if (enemyStats != null && player != null)
        {
            GameObject playerObj = player.gameObject;
            //LMJ: Enemy.Attack() will be called from OnTriggerStay
            //LMJ: Just set animation here
            if (animator != null)
            {
                animator.SetTrigger("IsAttack");
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        //LMJ: Calculate movement speed for animation
        float currentSpeed = 0f;

        if (characterController != null && characterController.enabled)
        {
            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0;
            currentSpeed = horizontalVelocity.magnitude;
        }

        animator.SetFloat("Speed", currentSpeed);

        //LMJ: Reset attack animation when not in range
        if (!isInAttackRange && animator.GetBool("IsAttack"))
        {
            // animator.SetTrigger("ResetAttack");
        }
    }

    private void HandleFallDeath()
    {
        //LMJ: Monster fell off the map - trigger death
        Debug.LogWarning($"{gameObject.name} fell below death zone (Y: {transform.position.y}). Destroying...");

        //LMJ: Trigger death through Enemy stats system if available
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(9999f, gameObject);
        }
        else
        {
            //LMJ: Direct destroy if no stats component
            Destroy(gameObject);
        }
    }

    protected override void OnReachedDestination()
    {
        //LMJ: Don't log when reaching destination during chase
        isFollowingPath = false;
        currentPath = null;
        currentWaypointIndex = 0;
    }

    //LMJ: Override to disable interaction object checking for enemies
    protected override bool CheckForInteractionObject(Vector3 targetPosition)
    {
        return false; //LMJ: Enemies don't interact with doors/jumps
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        //LMJ: Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        //LMJ: Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        //LMJ: Draw line to player
        if (player != null)
        {
            Gizmos.color = isChasing ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
