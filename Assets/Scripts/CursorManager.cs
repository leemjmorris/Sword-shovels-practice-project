using UnityEngine;

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
}
