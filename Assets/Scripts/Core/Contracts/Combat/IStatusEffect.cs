using UnityEngine;

namespace Core.Contracts.Combat
{
    public interface IStatusEffect
    {
        void OnApply(GameObject target);
        void Tick(float dt);
        bool IsFinished { get; }
        void OnRemove();
    }
}
