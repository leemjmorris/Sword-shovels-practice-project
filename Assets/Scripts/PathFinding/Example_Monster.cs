using UnityEngine;

namespace PathFinding
{
    //LMJ: Example monster AI that automatically chases player
    public class Example_Monster : PathfindableEntity
    {
        [Header("Monster AI")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float chaseRange = 15f;
        [SerializeField] private float pathUpdateInterval = 0.5f;
        [SerializeField] private Transform targetPlayer;

        private float lastPathUpdateTime;
        private bool isChasing = false;

        protected override void Update()
        {
            base.Update();

            if (targetPlayer == null)
            {
                FindPlayer();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (!isChasing && distanceToPlayer <= detectionRange)
            {
                StartChasing();
            }

            if (isChasing && distanceToPlayer > chaseRange)
            {
                StopChasing();
            }

            if (isChasing && Time.time - lastPathUpdateTime >= pathUpdateInterval)
            {
                UpdatePath();
                lastPathUpdateTime = Time.time;
            }
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }

        private void StartChasing()
        {
            isChasing = true;
            Debug.Log($"{gameObject.name}: Started chasing player!");
            UpdatePath();
        }

        private void StopChasing()
        {
            isChasing = false;
            StopMoving();
            Debug.Log($"{gameObject.name}: Stopped chasing player!");
        }

        private void UpdatePath()
        {
            if (targetPlayer != null)
            {
                MoveTo(targetPlayer.position);
            }
        }

        protected override void OnReachedDestination()
        {
            base.OnReachedDestination();

            if (isChasing && targetPlayer != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= chaseRange)
                {
                    UpdatePath();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
    }
}
