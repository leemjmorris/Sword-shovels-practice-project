using UnityEngine;

public class CharacterStats : MonoBehaviour
{   
    [SerializeField] protected Animator animator;
    public StatData statsData;
    protected float maxHealth;
    protected float currentHealth;
    protected float attackPower;
    protected float defense;
    protected float attackCooldown;

    /// <summary>
    /// Init Stat
    /// </summary>
    protected virtual void InitStats()
    {
        maxHealth = (int)statsData.maxHealth;
        currentHealth = maxHealth;
        defense = statsData.defense;
        attackPower = statsData.attackPower;
        attackCooldown = statsData.attackCooldown;

    }

    /// <summary>
    /// Take Damage
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    /// <param name="go">The game object dealing the damage.</param>
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

    /// <summary>
    /// Attack
    /// </summary>
    /// <param name="target">The game object to attack.</param>
    protected virtual void Attack(GameObject target)
    {
        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(attackPower, target);
            animator.SetTrigger("IsAttack");
            Debug.Log($"{statsData.name} attacked {target.name}!");
        }
    }

    /// <summary>
    /// Die
    /// </summary>
    /// <param name="go">The game object that is dying.</param>
    protected virtual void Die(GameObject go)
    {
        go.SetActive(false);
        Destroy(go);   
    }
   
} 
