using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : CharacterStats
{
    private float lastAttackTime = 0f;
    private GameObject currentEnemy = null;

    [SerializeField] private Transform weaponPoint;
    [SerializeField] private GameObject swordPrefab; 
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
        // 이동 중이 아닐 때만 마우스 위치를 바라보도록 회전
        if (!IsMoving())
        {
            LookAtMousePosition();
        }
        
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
                else
                {
                    //JML : Move player to clicked point
                    PlayerMovement playerMovement = GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.MoveTo(hit.point);
                    }
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
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentEnemy = other.gameObject;
        }    
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentEnemy = null;
        }
    }
    
    private void LookAtMousePosition()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 mouseOffset = (Vector2)Input.mousePosition - screenCenter;
        mouseOffset.x /= Screen.width * 0.5f;
        mouseOffset.y /= Screen.height * 0.5f;
        
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraForward = Camera.main.transform.forward;
        
        cameraRight.y = 0;
        cameraForward.y = 0;
        cameraRight.Normalize();
        cameraForward.Normalize();
        
        Vector3 targetDirection = (cameraRight * mouseOffset.x + cameraForward * mouseOffset.y).normalized;
        
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    private bool IsMoving()
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            return !playerMovement.HasReachedDestination();
        }
        return false;
    }
}

