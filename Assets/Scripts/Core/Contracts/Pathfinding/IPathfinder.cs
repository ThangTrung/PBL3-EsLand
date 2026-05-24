using System.Collections.Generic;
using UnityEngine;

namespace Core.Contracts.Pathfinding
{
    public interface IPathfinder
    {
        /// <summary>
        /// Calculates a path from the start position to the target position.
        /// </summary>
        /// <param name="startPosition">The starting world position.</param>
        /// <param name="targetPosition">The desired destination world position.</param>
        /// <param name="entityRadius">The physical radius of the entity to avoid narrow gaps. Default is 0 (ignores size).</param>
        /// <returns>A list of waypoints representing the path, or an empty list if no path is found.</returns>
        List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition, float entityRadius = 0f);
    }
}