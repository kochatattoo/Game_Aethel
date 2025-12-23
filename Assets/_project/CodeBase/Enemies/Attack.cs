using CodeBase.Hero;
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
        public float Radius = 1f;
        public float EffectiveDistance = 1f;
        public Transform AttackPoint;

        private Transform _heroTransform;
        private HeroDeath _heroDeath;

        private readonly Collider[] _hits = new Collider[1];
        private int _layerMask;
        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        public void Construct(Transform heroTransform, HeroDeath heroDeath)
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");
            _heroTransform = heroTransform;

            _heroDeath = heroDeath;
            _heroDeath.PlayerDie += OnPlayerDie;
        }


        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        private void OnDisable()
        {
            _heroDeath.PlayerDie -= OnPlayerDie;
        }

        private void OnPlayerDie()
        {
            Animator.PlayWin();
            this.enabled = false;
        }

        private void OnAttack()
        {
            if (Hit(out Collider hit))
            {
                PhysicsDebug.DrawDebug(StartPosition(), Radius, 1f);

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
            int hitcount = Physics.OverlapSphereNonAlloc(StartPosition(), Radius, _hits, _layerMask);

            hit = _hits.FirstOrDefault();

            return hitcount > 0;
        }

        private Vector3 StartPosition() =>
            new Vector3(AttackPoint.position.x, AttackPoint.position.y, AttackPoint.position.z);

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