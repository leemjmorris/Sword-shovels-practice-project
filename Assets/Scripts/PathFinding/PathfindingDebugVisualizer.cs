using UnityEngine;

namespace PathFinding
{
    //LMJ: Runtime debug visualizer for pathfinding system
    public class PathfindingDebugVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showGridAtRuntime = false;
        [SerializeField] private bool showPathsAtRuntime = true;
        [SerializeField] private bool showPathfindingInfo = true;

        [Header("References")]
        [SerializeField] private PathFindManager pathFindManager;
        [SerializeField] private GridManager gridManager;

        [Header("UI Settings")]
        [SerializeField] private float infoDisplayX = 10f;
        [SerializeField] private float infoDisplayY = 10f;
        [SerializeField] private int fontSize = 14;

        private GUIStyle guiStyle;

        private void Start()
        {
            //LMJ: Find managers if not assigned
            if (pathFindManager == null)
            {
                pathFindManager = PathFindManager.Instance;
            }

            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();
            }

            //LMJ: Setup GUI style
            guiStyle = new GUIStyle();
            guiStyle.fontSize = fontSize;
            guiStyle.normal.textColor = Color.white;
            guiStyle.padding = new RectOffset(10, 10, 10, 10);
        }

        private void Update()
        {
            // Toggle grid visualization with G key
            if (Input.GetKeyDown(KeyCode.G))
            {
                showGridAtRuntime = !showGridAtRuntime;
                if (gridManager != null)
                {
                    gridManager.ToggleGridVisualization(showGridAtRuntime);
                }
                Debug.Log($"Grid visualization: {(showGridAtRuntime ? "ON" : "OFF")}");
            }

            // Toggle path visualization with P key
            if (Input.GetKeyDown(KeyCode.P))
            {
                showPathsAtRuntime = !showPathsAtRuntime;
                Debug.Log($"Path visualization: {(showPathsAtRuntime ? "ON" : "OFF")}");
            }

            // Toggle info display with I key
            if (Input.GetKeyDown(KeyCode.I))
            {
                showPathfindingInfo = !showPathfindingInfo;
                Debug.Log($"Info display: {(showPathfindingInfo ? "ON" : "OFF")}");
            }

            // Cycle through pathfinding modes with M key
            if (Input.GetKeyDown(KeyCode.M) && pathFindManager != null)
            {
                PathfindingMode currentMode = pathFindManager.GetLastUsedMode();
                PathfindingMode newMode = GetNextMode(currentMode);
                pathFindManager.SetDefaultMode(newMode);
                Debug.Log($"Pathfinding mode changed: {currentMode} → {newMode}");
            }
        }

        private PathfindingMode GetNextMode(PathfindingMode currentMode)
        {
            switch (currentMode)
            {
                case PathfindingMode.NavMeshOnly:
                    return PathfindingMode.FloodfillOnly;
                case PathfindingMode.FloodfillOnly:
                    return PathfindingMode.Hybrid;
                case PathfindingMode.Hybrid:
                    return PathfindingMode.NavMeshOnly;
                default:
                    return PathfindingMode.Hybrid;
            }
        }

        private void OnGUI()
        {
            if (!showPathfindingInfo || pathFindManager == null)
                return;

            string info = "=== PATHFINDING DEBUG INFO ===\n\n";
            info += $"Current Mode: {pathFindManager.GetLastUsedMode()}\n\n";
            info += "Controls:\n";
            info += "[G] Toggle Grid Visualization\n";
            info += "[P] Toggle Path Visualization\n";
            info += "[I] Toggle This Info Display\n";
            info += "[M] Cycle Pathfinding Modes\n\n";

            if (gridManager != null)
            {
                info += $"Grid Size: {gridManager.GridSize.x} x {gridManager.GridSize.y}\n";
                info += $"Cell Size: {gridManager.CellSize}m\n";
            }

            info += $"\nPath Colors:\n";
            info += "NavMesh: Green\n";
            info += "Floodfill: Blue\n";
            info += "Hybrid: Cyan\n";

            // Draw semi-transparent background
            GUI.Box(new Rect(infoDisplayX - 5, infoDisplayY - 5, 350, 320), "");
            GUI.Label(new Rect(infoDisplayX, infoDisplayY, 340, 310), info, guiStyle);
        }

        //LMJ: Draw grid in scene view (editor only)
        private void OnDrawGizmos()
        {
            if (!showGridAtRuntime || gridManager == null)
                return;

            // Grid is already drawn by GridManager
        }
    }
}
