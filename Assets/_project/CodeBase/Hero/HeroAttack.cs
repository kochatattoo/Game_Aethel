using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services;
using CodeBase.Logic;
using UnityEngine;
using CodeBase.Enemies;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(HeroAnimator), typeof(CharacterController))]
    public class HeroAttack : MonoBehaviour, ISavedProgressReader
    {
        public HeroAnimator HeroAnimator;
        public Transform AttackPoint;
        public CharacterController CharacterController;

        public float Cleavage = 0.5f;

        private IInputService _input;

        private static int _layerMask;
        private Collider[] _hits = new Collider[3];
        private Stats _stats;

        public void Construct(IInputService input)
        {
            _input = input;
            _input.Attack += Attack;

            _layerMask = 1 << LayerMask.NameToLayer("Hittable");
        }

        private void OnDisable()
        {
            _input.Attack -= Attack;
        }

        private void Attack()
        {
            if (!HeroAnimator.IsAttacking)
                HeroAnimator.PlayAttack();
        }

        public void OnAttack()
        {
            PhysicsDebug.DrawDebug(StartPosition(), Cleavage, 1f);

            for (int i = 0; i < Hit(); i++)
            {
                _hits[i].transform.parent.GetComponent<IHealth>().TakeDamage(_stats.Damage);
            }
        }

        private int Hit() =>
            Physics.OverlapSphereNonAlloc(StartPosition(), _stats.DamageRadius, _hits, _layerMask);

        private Vector3 StartPosition() =>
            new Vector3(AttackPoint.position.x, AttackPoint.position.y, AttackPoint.position.z);

        public void LoadProgress(PlayerProgress progress)
        {
            _stats = progress.HeroStats;
        }
    }
}
