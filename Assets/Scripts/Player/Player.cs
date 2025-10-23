using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : CharacterStats
{
    private float lastAttackTime = 0f;
    private bool isNearEnemy = false;
    private GameObject currentEnemy = null;

    [SerializeField] private Transform weaponPoint; // 무기 장착 위치
    [SerializeField] private GameObject swordPrefab; // 검 프리팹
    private void Awake()
    {
        InitStats();
    }
    private void Start()
    {
        swordPrefab.transform.position = weaponPoint.position;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(100f, gameObject);
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.CompareTag("Enemy") && hit.collider.gameObject == currentEnemy)
                {
                    Attack(hit.collider.gameObject);
                    lastAttackTime = Time.time;
                    Debug.Log("Player Attack!");
                }
            }
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
            isNearEnemy = true;        
            currentEnemy = other.gameObject; 
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Enemy"))
        {
            isNearEnemy = false;
            currentEnemy = null;
        }
    }
}

