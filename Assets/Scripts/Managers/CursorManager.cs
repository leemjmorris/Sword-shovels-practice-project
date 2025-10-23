using UnityEngine;

public enum CursorState
{
    Door,
    Pointer,
    Attack,
    Target
}

public static class CursorManager
{
    /// <summary>
    /// Locks and hides the cursor to match the gameplay state.
    /// </summary>
    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Unlocks and shows the cursor to match the gameplay state.
    /// </summary>
    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void SetCursorIcon(CursorState state)
    {
        switch (state)
        {
            case CursorState.Door:
                // Set cursor icon for Door
                break;
            case CursorState.Pointer:
                // Set cursor icon for Pointer
                break;
            case CursorState.Attack:
                // Set cursor icon for Attack
                break;
            case CursorState.Target:
                // Set cursor icon for Target
                break;
            default:
                break;
        }
    }
}
