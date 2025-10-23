using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LongDistanceMonster : CharacterStats
{
    [Header("Target")]
    [SerializeField] private GameObject player;

    [Header("Navigation")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private float detectionRange = 20f;    
    [SerializeField] private float attackRange = 15f;      

    [Header("Ranged Attack")]
    [SerializeField] private GameObject attackEffectPrefab; 
    [SerializeField] private float attackDelay = 1f;       
    [SerializeField] private float effectDuration = 2f;    

    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        InitStats();
        cancellationTokenSource = new CancellationTokenSource();
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange && 
            distanceToPlayer <= attackRange && 
            Time.time >= lastAttackTime + attackCooldown && 
            !isAttacking)
        {
            PerformRangedAttack(cancellationTokenSource.Token).Forget();
        }
    }

    protected override void InitStats()
    {
        base.InitStats();
    }

    public override void TakeDamage(float damage, GameObject go)
    {
        base.TakeDamage(damage, go);
    }

    protected override void Attack(GameObject target)
    {
        if (animator != null)
        {
            animator.SetTrigger("IsAttack");
        }
        Debug.Log($"{statsData.name} is casting ranged attack on {target.name}!");
    }

    private async UniTaskVoid PerformRangedAttack(CancellationToken cancellationToken)
    {
        if (playerTransform == null) return;

        isAttacking = true;
        lastAttackTime = Time.time;


        if (animator != null)
        {
            animator.SetTrigger("IsAttack");
        }

        try
        {

            await UniTask.Delay(System.TimeSpan.FromSeconds(attackDelay), cancellationToken: cancellationToken);


            Vector3 attackPosition = playerTransform.position;

            if (attackEffectPrefab != null)
            {
                GameObject effect = Instantiate(attackEffectPrefab, attackPosition, Quaternion.identity);


                ApplyDamageAtPosition(attackPosition);


                if (effect != null)
                {
                    Destroy(effect, effectDuration);
                }
            }
            else
            {
                ApplyDamageAtPosition(attackPosition);
                Debug.Log($"Ranged attack hit at position: {attackPosition}");
            }
        }
        catch (System.OperationCanceledException)
        {
            //JML : CancellationToken => print Log
            Debug.Log("Ranged attack was cancelled");
        }
        finally
        {
            isAttacking = false;
        }
    }
    
    private void ApplyDamageAtPosition(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 2f);
        
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                CharacterStats playerStats = hitCollider.GetComponent<CharacterStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackPower, player);
                    Debug.Log($"{statsData.name} dealt {attackPower} damage to {hitCollider.name}!");
                }
            }
        }
    }

    protected override void Die(GameObject go)
    {
        base.Die(go);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
