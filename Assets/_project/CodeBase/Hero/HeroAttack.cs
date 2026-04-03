using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;
using CodeBase.Enemies;
using VFXSystem.Processors;
using VFXSystem.Resolver;
using VFXSystem.Components;
using VFXSystem.Service;
using VFXSystem.Sensors;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(HeroAnimator))]
    public class HeroAttack : MonoBehaviour, ISavedProgressReader
    {
        [SerializeField] private HeroAnimator _heroAnimator;
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _cleavage = 0.5f;

        public float Cleavage { get => _cleavage; }
        public float AttackCooldown { get => _attackCooldown; }
        public float AttackRange { get => _attackRange; }

        private readonly Collider[] _hits = new Collider[3];
        private Stats _stats;
        private IVFXProcessor _vFXProcessor;

        private static int _layerMask;

        public void Construct(IVFXFacade facade)
        {
            _layerMask = 1 << LayerMask.NameToLayer("Hittable");
            _vFXProcessor = new VFXProcessor(facade);
        }

        public void Attack(Transform enemy)
        {
            transform.LookAt(enemy);
            if (!_heroAnimator.IsAttacking)
                _heroAnimator.PlayAttack();
        }

        public void OnAttack()
        {
            PhysicsDebug.DrawDebug(StartPosition(), _cleavage, 1f);
            Vector3 origin = StartPosition();

            Vector3 attackDirection = transform.forward;

            for (int i = 0; i < Hit(); i++)
            {
                Collider targetCollider = _hits[i];

                if(targetCollider.transform.parent.TryGetComponent<IHealth>(out IHealth health))
                {
                    health.TakeDamage(_stats.Damage); 
                }

                // --- ПРИМЕНЕНИЕ СЕНСОРОВ ---
                Vector3 hitPoint;
                Vector3 hitNormal;

                // Пытаемся получить точную точку через Raycast в сторону коллайдера
                // Высчитываем направление от игрока к центру врага
                Vector3 dirToTarget = (targetCollider.bounds.center - origin).normalized;

                if (VFXHitSensor.GetPreciseHit(origin, dirToTarget, _cleavage + 1f, targetCollider.gameObject.layer, out RaycastHit preciseHit))
                {
                    // Если луч попал точно — берем данные из RaycastHit
                    hitPoint = preciseHit.point;
                    hitNormal = preciseHit.normal;
                }
                else
                {
                    // Фолбэк: если луч почему-то пролетел мимо, используем наш быстрый метод
                    VFXHitSensor.GetContactData(targetCollider, origin, out hitPoint, out hitNormal);
                }
                // ---------------------------

                GameObject hitObject = targetCollider.gameObject;

                VFXPointData vFXPointData;
                if (targetCollider.TryGetComponent<IHitbox>(out IHitbox hitbox))
                { 
                    vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal, 1f, hitbox.MaterialType); 
                }
                else
                {
                    vFXPointData = new VFXPointData(hitObject, hitPoint, hitNormal);
                }

                _vFXProcessor?.PlayVFX(vFXPointData);
            }
        }

        public void LoadProgress(PlayerProgress progress) => 
            _stats = progress.HeroStats;

        private int Hit() =>
            Physics.OverlapSphereNonAlloc(StartPosition(), _stats.DamageRadius, _hits, _layerMask);

        private Vector3 StartPosition() =>
            new Vector3(_attackPoint.position.x, _attackPoint.position.y, _attackPoint.position.z);
    }
}
