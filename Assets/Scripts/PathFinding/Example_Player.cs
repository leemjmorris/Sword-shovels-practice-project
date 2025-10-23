using UnityEngine;

namespace PathFinding
{
    //LMJ: Example player implementation with right-click movement
    public class Example_Player : PathfindableEntity
    {
        [Header("Player Input")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer;

        protected override void Awake()
        {
            base.Awake();

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetMouseButtonDown(1))
            {
                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Debug.Log($"Player moving to: {hit.point}");
                MoveTo(hit.point);
            }
        }

        protected override void OnReachedDestination()
        {
            base.OnReachedDestination();
            Debug.Log("Player arrived at destination!");
        }
    }
}
