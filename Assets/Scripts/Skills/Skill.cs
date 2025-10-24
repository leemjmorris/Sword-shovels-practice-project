using UnityEngine;
using Cysharp.Threading.Tasks;

//LMJ: Base class for all skills
public abstract class Skill : MonoBehaviour
{
    [Header("Skill Info")]
    [SerializeField] protected string skillName;
    [SerializeField] protected float lifetime = 5f;

    protected bool isActive = false;

    //LMJ: Initialize skill when spawned from pool
    public virtual void Initialize(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        isActive = true;
        gameObject.SetActive(true);

        OnSkillActivated();
    }

    //LMJ: Called when skill is activated
    protected virtual void OnSkillActivated()
    {
        //LMJ: Override in child classes
    }

    //LMJ: Execute the skill (can be overridden for custom behavior)
    public virtual async UniTask ExecuteSkill()
    {
        await UniTask.Delay((int)(lifetime * 1000));
        DeactivateSkill();
    }

    //LMJ: Deactivate and return to pool
    public virtual void DeactivateSkill()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    //LMJ: Called when skill hits something
    protected virtual void OnSkillHit(Collider other)
    {
        //LMJ: Override in child classes for damage, effects, etc.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
        {
            OnSkillHit(other);
        }
    }
}
