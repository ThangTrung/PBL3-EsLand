using UnityEngine;

namespace Core.Contracts.AI
{
    /// <summary>
    /// Contract for character navigation in a 2D environment.
    /// Abstracts the underlying pathfinding system (like NavMesh).
    /// </summary>
    public interface ICharacterNavigator
    {
        /// <summary>
        /// Sets the target destination in 2D space (XY).
        /// </summary>
        void SetDestination(Vector2 destination);

        /// <summary>
        /// Gets the normalized direction vector towards the next waypoint in 2D space.
        /// </summary>
        Vector2 GetNextDirection();

        /// <summary>
        /// Stops the navigation and clears the current path.
        /// </summary>
        void Stop();

        /// <summary>
        /// Checks if the character has reached the destination or current path is finished.
        /// </summary>
        bool IsAtDestination(float stopDistance);

        /// <summary>
        /// Syncs the navigator position with the character's current 2D position.
        /// </summary>
        void SyncPosition();
    }
}
