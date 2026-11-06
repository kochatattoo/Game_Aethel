using CodeBase.Logic;
using UnityEngine;
using VFXSystem.Processors;
using VFXSystem.Service;
using CodeBase.Sensors;

namespace CodeBase.Hero
{
    public class HeroAttackRuntime : MonoBehaviour
    {
        [SerializeField] //TODO: Сделать для правой и для левой руки, можно передавать ID руки при ивенте
        private BladeSensor _bladeSensor;

        private IVFXProcessor _vFXProcessor;
        private HeroAttack _heroAttack;
        private readonly float _impactStrenght = 1.0f;

        public void Construct(IVFXFacade facade, HeroAttack heroAttack)
        {
            _vFXProcessor = new VFXProcessor(facade);
            _heroAttack = heroAttack;   

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
            if (targetCollider.transform.parent.TryGetComponent<IHealth>(out IHealth health))
                health.TakeDamage(_heroAttack.Damage);

            _vFXProcessor?.PlayVFX(targetCollider, bladePos, _impactStrenght);
        }
    }
}
