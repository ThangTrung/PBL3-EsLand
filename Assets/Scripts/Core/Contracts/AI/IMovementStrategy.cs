using UnityEngine;

namespace Core.Contracts.AI
{
    public interface IMovementStrategy
    {
        void Move(Vector3 direction);
        void StopMovement();
    }
}
