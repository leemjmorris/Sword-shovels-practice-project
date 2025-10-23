using UnityEngine;

public class CharacterStats : MonoBehaviour
{   
    public StatData statsData;
    protected float maxHealth;
    protected float currentHealth;
    protected float attackPower;
    protected float defense;
    protected float attackCooldown;
    protected virtual void InitStats()
    {
        maxHealth = (int)statsData.maxHealth;
        currentHealth = maxHealth;
        defense = statsData.defense;
        attackPower = statsData.attackPower;
        attackCooldown = statsData.attackCooldown;

    }

    public virtual void TakeDamage(float damage, GameObject go)
    {
        float finalDamage = Mathf.Max(damage - defense, 1);

        currentHealth -= finalDamage;
        Debug.Log($"{statsData.name} took {finalDamage} damage. Current Health: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(go);
        }
    }

    protected virtual void Attack(GameObject target)
    {
        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(attackPower, target);
            Debug.Log($"{statsData.name} attacked {target.name}!");
        }
    }


    protected virtual void Die(GameObject go)
    {
        go.SetActive(false);
        Destroy(go);   
    }
   
} 
