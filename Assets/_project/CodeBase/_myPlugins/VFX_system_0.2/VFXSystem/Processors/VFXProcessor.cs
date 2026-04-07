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
        private readonly InteractResolver _resolver;

        public VFXProcessor(IVFXFacade facade)
        {
            _facade = facade;
            _resolver = new InteractResolver();
        }

        public void PlayVFX(VFXPointData data)
        {
            Debug.Log($"{data.MaterialType}, {data.GameObject}, {data.Position}, {data.Normal}, {data.Interact}");

            Vector3 point = data.Position;
            Vector3 normal = data.Normal;
            Transform parent = data.Parent;
            Quaternion rotation = data.HitRotation;
            float interact = data.Interact;

            if (data.MaterialType != BaseTypes.MaterialType.NoneDetected)
            {
                _facade.CreateVFX(data);
            }
            else
            {
                Material type = _resolver.MaterialResolve(data.GameObject);
                _facade.CreateVFX(type, point, normal, data.Parent, interact);
            }
        }

        public VFXPointData ResolveVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1f)
        {
            VFXHitSensor.GetSurfacePoint(targetCollider, bladePos,
                out Vector3 hitPoint,
                out Vector3 hitNormal,
                out Quaternion hitRotation);

            GameObject hitObject = targetCollider.gameObject;

            VFXPointData vFXPointData;
            if (targetCollider.TryGetComponent<IHitbox>(out IHitbox hitbox))
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation, impactStrenght, hitbox.MaterialType);
            }
            else
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation, impactStrenght);
            }

            return vFXPointData;
        }

        public void PlayVFX(Collider targetCollider, Vector3 bladePos, float impactStrenght = 1f) => 
            PlayVFX(ResolveVFX(targetCollider, bladePos, impactStrenght));

    }
}
