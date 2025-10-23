using UnityEngine;

public class Enemy : CharacterStats
{
    private float lastAttackTime = 0f;
    private void Awake()
    {
        InitStats();
    }

    private void Start()
    {

    }
    
    private void Update()
    {

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
        base.Attack(target);
    }

    protected override void Die(GameObject go)
    {
        base.Die(go);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack(other.gameObject);
            lastAttackTime = Time.time;
        }
    }
    private void  OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("IsAttack", false);
        }
    }
}
