using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : CharacterStats
{
    private float lastAttackTime = 0f;
    private void Awake()
    {
        InitStats();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(100f, gameObject);
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
        base.Attack(target);
    }

    protected override void Die(GameObject go)
    {
        base.Die(go);
        Debug.Log("Player has died.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(other.gameObject);
                lastAttackTime = Time.time;
                Debug.Log("Player Attack!");
            }
        }
    }
    private void  OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Enemy"))
        {
            animator.SetBool("IsAttack", false);
        }
    }
}

