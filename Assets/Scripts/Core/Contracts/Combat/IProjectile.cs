using Data.Combat;
using UnityEngine;

namespace Core.Contracts.Combat
{
    public interface IProjectile
    {
        void Initialize(ProjectileSpec spec, Transform owner, Transform target);
    }
}
