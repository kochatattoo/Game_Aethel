using CodeBase.Logic;
using UnityEngine;
using VFXSystem.Processors;
using VFXSystem.Resolver;
using VFXSystem.Components;
using VFXSystem.Service;
using VFXSystem.Sensors;
using CodeBase.Sensors;

namespace CodeBase.Hero
{
    public class HeroAttackRuntime : MonoBehaviour
    {
        [SerializeField] //TODO: Сделать для правой и для левой руки, можно передавать ID руки при ивенте
        private BladeSensor _bladeSensor;

        private IVFXProcessor _vFXProcessor;
        private HeroAttack _heroAttack;

        public void Construct(IVFXFacade facade, HeroAttack heroAttack)
        {
            _vFXProcessor = new VFXProcessor(facade);
            _heroAttack = heroAttack;   

            _bladeSensor.StopSensing();
        }

        // Вызывается из Animation Event в начале взмаха
        public void OpenAttackWindow() => _bladeSensor.StartSensing(ProcessHit);

        // Вызывается из Animation Event в конце взмаха
        public void CloseAttackWindow() => _bladeSensor.StopSensing();

        private void ProcessHit(Collider targetCollider, Vector3 bladePos)
        {
            if (targetCollider.transform.parent.TryGetComponent<IHealth>(out IHealth health))
                health.TakeDamage(_heroAttack.Damage);

            VFXHitSensor.GetSurfacePoint(targetCollider, bladePos, 
                out Vector3 hitPoint, 
                out Vector3 hitNormal, 
                out Quaternion hitRotation);

            GameObject hitObject = targetCollider.gameObject;

            VFXPointData vFXPointData;
            if (targetCollider.TryGetComponent<IHitbox>(out IHitbox hitbox))
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation, 1f, hitbox.MaterialType);
            }
            else
            {
                vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, hitRotation);
            }
            _vFXProcessor?.PlayVFX(vFXPointData);
        }
    }
}
