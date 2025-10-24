using UnityEngine;
using Cysharp.Threading.Tasks;

//LMJ: Example projectile skill (fireball, arrow, etc.)
public class ProjectileSkill : Skill
{
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool destroyOnHit = true;

    private Vector3 moveDirection;

    protected override void OnSkillActivated()
    {
        base.OnSkillActivated();

        //LMJ: Move in forward direction
        moveDirection = transform.forward;
    }

    private void Update()
    {
        if (isActive)
        {
            //LMJ: Move projectile forward
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    protected override void OnSkillHit(Collider other)
    {
        //LMJ: Check if hit enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, gameObject);
            }

            if (destroyOnHit)
            {
                DeactivateSkill();
            }
        }
    }
}
