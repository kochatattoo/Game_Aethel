using CodeBase.Enemies;
using CodeBase.Logic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Attack : MonoBehaviour
    {
        public EnemyAnimator Animator;

        public float AttackCooldown = 3f;

        public float Damage = 10;
        public float Cleavage = 1f;
        public float EffectiveDistance = 1f;

        private Transform _heroTransform;
        private readonly Collider[] _hits = new Collider[1];
        private int _layerMask;
        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        public void Construct(Transform heroTransform)
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");
            _heroTransform = heroTransform;
        }

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        private void OnAttack()
        {
            if (Hit(out Collider hit))
            {
                PhysicsDebug.DrawDebug(StartPoint(), Cleavage, 1f);

                hit.transform.GetComponent<IHealth>().TakeDamage(Damage);
            }
        }

        private void OnAttackEnded()
        {
            _attackCooldown = AttackCooldown;
            _isAttacking = false;
        }

        public void ResetAttack()
        {
            OnAttackEnded();
        }

        public void DisableAttack() =>
            _attackIsActive = false;

        public void EnabledAttack() =>
            _attackIsActive = true;

        private bool Hit(out Collider hit)
        {
            int hitcount = Physics.OverlapSphereNonAlloc(StartPoint(), Cleavage, _hits, _layerMask);

            hit = _hits.FirstOrDefault();

            return hitcount > 0;
        }

        private Vector3 StartPoint() =>
            new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z) + transform.forward * EffectiveDistance;

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
                _attackCooldown -= Time.deltaTime;
        }

        private bool CooldownIsUp() =>
            _attackCooldown <= 0f;
        private bool CanAttack() =>
           _attackIsActive && !_isAttacking && CooldownIsUp();

        private void StartAttack()
        {
            transform.LookAt(_heroTransform);
            Animator.PlayAttack1();
            _isAttacking = true;
        }
    }
}