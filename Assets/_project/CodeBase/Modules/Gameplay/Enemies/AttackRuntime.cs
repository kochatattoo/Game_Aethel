using CodeBase.Logic;
using CodeBase.Sensors;
using UnityEngine;
using VFXSystem.Processors;
using VFXSystem.Service;

namespace CodeBase.Enemies
{
    public class AttackRuntime : MonoBehaviour
    {
        [SerializeField] //TODO: Сделать для правой и для левой руки, можно передавать ID руки при ивенте
        private BladeSensor _bladeSensor;

        private IVFXProcessor _vFXProcessor;
        private IAttack _attack;
        private readonly float _impactStrenght = 1.0f;

        public void Construct(IVFXFacade facade, IAttack attack)
        {
            _vFXProcessor = new VFXProcessor(facade);
            _attack = attack;

            _bladeSensor.StopSensing();
        }

        public void OpenAttackWindow() => _bladeSensor.StartSensing(ProcessHit);

        public void CloseAttackWindow()
        {
            if (_bladeSensor != null)
            {
                _bladeSensor.StopSensing();
            }
        }

        private void ProcessHit(Collider targetCollider, Vector3 bladePos)
        {
            if (targetCollider == null) 
                return;

            IHealth health = targetCollider.GetComponentInParent<IHealth>();

            if (health != null)
            {
                health.TakeDamage(_attack.Damage);
            }

            _vFXProcessor?.PlayVFX(targetCollider, bladePos, _impactStrenght);
        }
    }
}