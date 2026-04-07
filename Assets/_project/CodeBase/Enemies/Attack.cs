using CodeBase.Configs;
using CodeBase.Hero;
using CodeBase.Logic;
using CodeBase.Sensors;
using System.Linq;
using UnityEngine;
using VFXSystem.Processors;
using VFXSystem.Service;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Attack : MonoBehaviour
    {
        [SerializeField]
        private EnemyAttackConfig _config;
        [SerializeField]
        private EnemyAnimator _animator;
        [SerializeField]
        private BladeSensor _bladeSensor;

        public Transform AttackPoint;

        private Transform _heroTransform;
        private HeroDeath _heroDeath;

        private readonly Collider[] _hits = new Collider[1];
        private int _layerMask;
        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        public float Damage => _config.Damage;

        public void Construct(Transform heroTransform, HeroDeath heroDeath)
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");
            _heroTransform = heroTransform;

            _heroDeath = heroDeath;
            _heroDeath.PlayerDie += OnPlayerDie;
        }

        public void ResetAttack()
        {
            OnAttackEnded();
        }

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
        }

        private void OnPlayerDie()
        {
            _animator.PlayWin();
            this.enabled = false;
        }

        private void OnAttack()
        {
            if (Hit(out Collider hit))
            {
                PhysicsDebug.DrawDebug(StartPosition(), _config.Radius, 1f);

                hit.transform.GetComponent<IHealth>().TakeDamage(_config.Damage);
            }
        }

        private void OnAttackEnded()
        {
            _attackCooldown = _config.AttackCooldown;
            _isAttacking = false;
        }

        private bool Hit(out Collider hit)
        {
            int hitcount = Physics.OverlapSphereNonAlloc(StartPosition(), _config.Radius, _hits, _layerMask);

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
            _animator.PlayAttack1();
            _isAttacking = true;
        }
    }

    public class EnemyAttackRuntime : MonoBehaviour
    {
        [SerializeField] //TODO: Сделать для правой и для левой руки, можно передавать ID руки при ивенте
        private BladeSensor _bladeSensor;

        private IVFXProcessor _vFXProcessor;
        private Attack _attack;
        private readonly float _impactStrenght = 1.0f;

        public void Construct(IVFXFacade facade, Attack attack)
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
            if (targetCollider.transform.parent.TryGetComponent<IHealth>(out IHealth health))
                health.TakeDamage(_attack.Damage);

            _vFXProcessor?.PlayVFX(targetCollider, bladePos, _impactStrenght);
        }
    }
}