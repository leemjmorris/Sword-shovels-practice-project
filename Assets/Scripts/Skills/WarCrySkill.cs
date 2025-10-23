using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

//LMJ: POE-style War Cry skill - AOE knockback effect
public class WarCrySkill : Skill
{
    [Header("War Cry Settings")]
    [SerializeField] private float radius = 10f;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float effectDuration = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject warCryEffect;
    [SerializeField] private float effectScale = 1f;

    private GameObject effectInstance;

    protected override void OnSkillActivated()
    {
        base.OnSkillActivated();

        //LMJ: Spawn visual effect
        SpawnEffect();

        //LMJ: Apply knockback to all enemies in range
        ApplyWarCryEffect();
    }

    private void SpawnEffect()
    {
        if (warCryEffect != null)
        {
            effectInstance = Instantiate(warCryEffect, transform.position, Quaternion.identity);
            effectInstance.transform.localScale = Vector3.one * effectScale * radius;

            //LMJ: Destroy effect after duration
            Destroy(effectInstance, effectDuration);
        }
        else
        {
            //LMJ: Create simple sphere visual if no effect assigned
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = transform.position;
            sphere.transform.localScale = Vector3.one * radius * 2f;

            //LMJ: Make it transparent and red
            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0f, 0f, 0.3f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }

            //LMJ: Remove collider
            Collider col = sphere.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Destroy(sphere, effectDuration);
        }
    }

    private void ApplyWarCryEffect()
    {
        //LMJ: Find all enemies in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                GameObject enemyObj = col.gameObject;

                //LMJ: Prevent hitting same enemy multiple times
                if (hitEnemies.Contains(enemyObj))
                    continue;

                hitEnemies.Add(enemyObj);

                //LMJ: Apply damage
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, gameObject);
                }

                //LMJ: Apply knockback
                ApplyKnockback(col);
            }
        }
    }

    private void ApplyKnockback(Collider enemyCollider)
    {
        //LMJ: Calculate knockback direction (away from skill center)
        Vector3 knockbackDirection = (enemyCollider.transform.position - transform.position).normalized;
        knockbackDirection.y = 0; //LMJ: Keep knockback horizontal

        //LMJ: Try to apply to CharacterController first
        CharacterController cc = enemyCollider.GetComponent<CharacterController>();
        if (cc != null && cc.enabled)
        {
            //LMJ: Apply knockback over time using coroutine
            StartKnockbackCoroutine(enemyCollider.gameObject, knockbackDirection).Forget();
        }
        else
        {
            //LMJ: Try Rigidbody
            Rigidbody rb = enemyCollider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    private async UniTaskVoid StartKnockbackCoroutine(GameObject enemy, Vector3 direction)
    {
        CharacterController cc = enemy.GetComponent<CharacterController>();
        if (cc == null || !cc.enabled)
            return;

        //LMJ: Apply knockback over short duration
        float elapsed = 0f;
        float knockbackDuration = 0.3f;

        while (elapsed < knockbackDuration)
        {
            if (enemy == null || cc == null || !cc.enabled)
                break;

            float force = Mathf.Lerp(knockbackForce, 0f, elapsed / knockbackDuration);
            Vector3 movement = direction * force * Time.deltaTime;
            movement.y = -9.81f * Time.deltaTime; //LMJ: Apply gravity

            cc.Move(movement);

            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
    }

    public override async UniTask ExecuteSkill()
    {
        //LMJ: Skill is instant, but wait for effect duration before deactivating
        await UniTask.Delay((int)(effectDuration * 1000));
        DeactivateSkill();
    }

    public override void DeactivateSkill()
    {
        base.DeactivateSkill();

        //LMJ: Clean up effect if still exists
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }

    //LMJ: Draw gizmo to show range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
