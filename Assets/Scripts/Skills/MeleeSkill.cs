using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

//LMJ: Example melee skill (slash, smash, etc.)
public class MeleeSkill : Skill
{
    [Header("Melee Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float activeTime = 0.5f;
    [SerializeField] private bool hitMultipleEnemies = true;

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    protected override void OnSkillActivated()
    {
        base.OnSkillActivated();

        //LMJ: Clear hit list
        hitEnemies.Clear();
    }

    public override async UniTask ExecuteSkill()
    {
        //LMJ: Stay active for a short time
        await UniTask.Delay((int)(activeTime * 1000));
        DeactivateSkill();
    }

    protected override void OnSkillHit(Collider other)
    {
        //LMJ: Check if hit enemy
        if (other.CompareTag("Enemy"))
        {
            GameObject enemyObj = other.gameObject;

            //LMJ: Check if already hit this enemy
            if (!hitMultipleEnemies && hitEnemies.Contains(enemyObj))
                return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, gameObject);
                hitEnemies.Add(enemyObj);
            }
        }
    }

    public override void DeactivateSkill()
    {
        base.DeactivateSkill();
        hitEnemies.Clear();
    }
}
