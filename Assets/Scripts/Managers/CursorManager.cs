using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D doorCursor;
    [SerializeField] private Texture2D pointerCursor;
    [SerializeField] private Texture2D attackCursor;
    

    private void Start()
    {
        UnlockCursor();
    }
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            int layer = hit.collider.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);
        
            switch (layerName)
            {
                case "Ground":
                    Debug.Log("Ground Layer Hit");
                    break;
                case "Door":
                    Debug.Log("Door Layer Hit");
                    break;
                case "Enemy":
                    Debug.Log("Enemy Layer Hit");
                    break;
                case "Pointer":
                    Debug.Log("Pointer Layer Hit");
                    break;
                default:
                    break;
            }
            Debug.Log($"Hit Layer: {layer} ({layerName})");
            Debug.Log($"Hit Object: {hit.collider.name}");
        }
        
    }

    /// <summary>
    /// Locks and hides the cursor to match the gameplay state.
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Unlocks and shows the cursor to match the gameplay state.
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
