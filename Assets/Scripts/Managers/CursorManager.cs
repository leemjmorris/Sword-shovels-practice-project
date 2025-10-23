using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D targetCursor;
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
                    Cursor.SetCursor(targetCursor, Vector2.zero, CursorMode.Auto);
                    break;
                case "Door":
                    Cursor.SetCursor(doorCursor, Vector2.zero, CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
                    break;
                default:
                    break;
            }
        }
        else
        {
            Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
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
