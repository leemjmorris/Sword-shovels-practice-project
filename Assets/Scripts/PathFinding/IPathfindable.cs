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
    }
}
