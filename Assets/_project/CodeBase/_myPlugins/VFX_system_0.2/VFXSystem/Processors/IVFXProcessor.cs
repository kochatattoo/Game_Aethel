using UnityEngine;
using VFXSystem.Resolver;

namespace VFXSystem.Processors
{
    public interface IVFXProcessor
    {
        void PlayVFX(VFXPointData data);
        void PlayVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1f);
        VFXPointData ResolveVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1);
    }
}
