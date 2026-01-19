using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;
using CodeBase.Enemies;

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

        private static int _layerMask;

        public void Construct()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Hittable");
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

            for (int i = 0; i < Hit(); i++)
            {
                _hits[i].transform.parent.GetComponent<IHealth>().TakeDamage(_stats.Damage);
            }
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _stats = progress.HeroStats;
        }

        private int Hit() =>
            Physics.OverlapSphereNonAlloc(StartPosition(), _stats.DamageRadius, _hits, _layerMask);

        private Vector3 StartPosition() =>
            new Vector3(_attackPoint.position.x, _attackPoint.position.y, _attackPoint.position.z);
    }
}
