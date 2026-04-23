using CodeBase.Configs;
using CodeBase.Hero;
using CodeBase.Logic.Animate;
using UnityEngine;
using VFXSystem.Service;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Attack : MonoBehaviour, IAttack
    {
        [SerializeField]
        private EnemyAttackConfig _config;
        [SerializeField]
        private EnemyAnimator _animator;
        [SerializeField]
        private AttackRuntime _attackRuntime;

        private Transform _heroTransform;
        private HeroDeath _heroDeath;

        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        public float Damage => _config.Damage;

        public void Construct(Transform heroTransform, HeroDeath heroDeath, IVFXFacade facade)
        {
            _heroTransform = heroTransform;

            _heroDeath = heroDeath;
            _heroDeath.PlayerDie += OnPlayerDie;
            _animator.StateExited += OnAnimatorStateExited;

            _attackRuntime.Construct(facade, this);
        }

        private void OnAnimatorStateExited(AnimatorState state)
        {
            if (state == AnimatorState.Attack)
            {
                OnAttackEnded();
            }
        }

        public void ResetAttack() => 
            OnAttackEnded();

        public void DisableAttack() =>
            _attackIsActive = false;

        public void EnabledAttack() =>
            _attackIsActive = true;

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        private void OnDisable()
        {
            _heroDeath.PlayerDie -= OnPlayerDie;
            _animator.StateExited -= OnAnimatorStateExited;
        }

        private void OnPlayerDie()
        {
            _animator.PlayWin();
            this.enabled = false;
        }

        private void OnAttack()
        {
           _attackRuntime.OpenAttackWindow();
        }

        private void OnAttackEnded()
        {
            _attackRuntime.CloseAttackWindow();

            _attackCooldown = _config.AttackCooldown;
            _isAttacking = false;
        }

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
            _animator.PlayAttack1();
            _isAttacking = true;
        }
    }
}