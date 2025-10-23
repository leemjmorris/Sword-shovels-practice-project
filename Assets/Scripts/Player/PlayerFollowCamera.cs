using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform target; // 플레이어 Transform
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0); // 기본 오프셋

    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (target == null)
        {
            target = transform;
        }
    }

    private void LateUpdate()
    {
        if (target != null && cameraTransform != null)
        {
            desiredPosition = target.position + offset;
            smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, smoothSpeed);
            cameraTransform.position = smoothedPosition;
        }
    }
}
