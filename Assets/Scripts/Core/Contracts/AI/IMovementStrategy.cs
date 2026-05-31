using UnityEngine;

namespace Core.Contracts.AI
{
    /// <summary>
    /// Contract for AI movement behaviors.
    /// </summary>
    public interface IMovementStrategy
    {
        void Move(Vector3 destination);
        void StopMovement();
    }
}
