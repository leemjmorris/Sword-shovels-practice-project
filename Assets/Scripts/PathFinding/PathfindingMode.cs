namespace PathFinding
{
    //LMJ: Defines different pathfinding modes
    public enum PathfindingMode
    {
        NavMeshOnly,        // Use only NavMesh (fast, static environments)
        FloodfillOnly,      // Use only Floodfill (precise, dynamic obstacles)
        Hybrid              // Combine NavMesh + Floodfill (best of both)
    }
}
