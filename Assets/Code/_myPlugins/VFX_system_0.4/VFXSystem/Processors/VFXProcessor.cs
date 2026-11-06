using UnityEngine;
using VFXSystem.Components;
using VFXSystem.Resolver;
using VFXSystem.Sensors;
using VFXSystem.Service;

namespace VFXSystem.Processors
{
    public class VFXProcessor : IVFXProcessor
    {
        private readonly IVFXFacade _facade;

        public VFXProcessor(IVFXFacade facade)
        {
            _facade = facade;
        }

        public void PlayVFX(VFXPointData data)
        {
            if (data.MaterialType == BaseTypes.MaterialType.None)
            {
                Material type = InteractResolver.MaterialResolve(data.GameObject);
                _facade.CreateVFX(type, data);
            }
            else
            {
                _facade.CreateVFX(data);
            }
        }

        public VFXPointData ResolveVFX(Collider targetCollider, Vector3 bladePos, float impactStrength = 1f)
        {
            VFXHitSensor.GetSurfacePoint(targetCollider, bladePos,
                out Vector3 hitPoint,
                out Vector3 hitNormal,
                out Quaternion hitRotation);

            GameObject hitObject = targetCollider.gameObject;

            VFXPointData vFXPointData;
            if (targetCollider.TryGetComponent<IHitbox>(out IHitbox hitbox))
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation, impactStrength, hitbox.MaterialType);
            }
            else
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation, impactStrength);
            }

            return vFXPointData;
        }

        public void PlayVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1f) => 
            PlayVFX(ResolveVFX(targetCollider, bladePos, impactStrenght));
    }
}
