using UnityEngine;
using Cysharp.Threading.Tasks;

//LMJ: Example buff/heal skill
public class BuffSkill : Skill
{
    [Header("Buff Settings")]
    [SerializeField] private float healAmount = 50f;
    [SerializeField] private float buffDuration = 5f;
    [SerializeField] private bool applyToPlayer = true;

    protected override void OnSkillActivated()
    {
        base.OnSkillActivated();

        if (applyToPlayer)
        {
            //LMJ: Find player and apply heal
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                CharacterStats stats = playerObj.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    //LMJ: Heal by dealing negative damage
                    stats.TakeDamage(-healAmount, gameObject);
                }
            }
        }
    }

    public override async UniTask ExecuteSkill()
    {
        //LMJ: Stay active for buff duration
        await UniTask.Delay((int)(buffDuration * 1000));
        DeactivateSkill();
    }
}
