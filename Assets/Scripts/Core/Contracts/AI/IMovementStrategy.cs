using UnityEngine;
using System;

namespace Core.Contracts.AI
{
    /// <summary>
    /// Contract for AI movement behaviors.
    /// </summary>
    public interface IMovementStrategy
    {
        void Move(Vector3 destination, float stopDistance = 0.5f, Action onReached = null);
        void Follow(Transform target, float stopDistance = 1.0f, Action onReached = null);
        void StopMovement();
    }
}
