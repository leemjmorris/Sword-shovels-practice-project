using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    //LMJ: Interface for all entities that can use pathfinding (Player, Monster, NPC)
    public interface IPathfindable
    {
        Vector3 Position { get; }
        float MoveSpeed { get; }
        void SetPath(List<Vector3> path);
        void FollowPath();
        bool HasReachedDestination();

        //LMJ: Get preferred pathfinding mode for this entity
        PathfindingMode GetPreferredPathfindingMode();

        //LMJ: Whether this entity requires dynamic obstacle avoidance
        bool RequiresDynamicObstacleAvoidance();
    }
}
