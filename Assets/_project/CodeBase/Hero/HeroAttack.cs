using CodeBase.Configs;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic.Animate;
using UnityEngine;
using VFXSystem.Service;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(HeroAnimator))]
    public class HeroAttack : MonoBehaviour, ISavedProgressReader
    {
        [SerializeField] 
        private HeroAnimator _heroAnimator;
        [SerializeField] 
        private HeroAttackRuntime _heroAttackRuntime;
        [SerializeField]
        private HeroAttackConfig _heroAttackConfig;

        private Stats _stats;

        public float AttackCooldown { get => _heroAttackConfig.AttackCooldown; }
        public float AttackRange { get => _heroAttackConfig.AttackRange; }
        public float Damage => _stats.Damage;

        public void Construct(IVFXFacade facade)
        { 
            _heroAttackRuntime.Construct(facade, this);
            _heroAnimator.StateExited += OnAnimatorStateExited;
        }

        private void OnAnimatorStateExited(AnimatorState state)
        {
            if (state == AnimatorState.Attack)
            {
                EndAttack();
            }
        }

        public void Attack(Transform enemy)
        {
            transform.LookAt(enemy);

            if (!_heroAnimator.IsAttacking)
                _heroAnimator.PlayAttack();
        }

        public void OnAttack() => 
            _heroAttackRuntime.OpenAttackWindow();

        public void EndAttack() => 
            _heroAttackRuntime.CloseAttackWindow(); //TODO: Обработка отмены атаки - вызов закрытия окна

        public void LoadProgress(PlayerProgress progress) => 
            _stats = progress.HeroStats;

        private void OnDisable()
        {
            EndAttack();

            if (_heroAnimator != null)
                _heroAnimator.StateExited -= OnAnimatorStateExited;
        }

        private void OnDestroy()
        {
            // Не забываем отписаться при уничтожении объекта
            if (_heroAnimator != null)
                _heroAnimator.StateExited -= OnAnimatorStateExited;
        }
    }
}
