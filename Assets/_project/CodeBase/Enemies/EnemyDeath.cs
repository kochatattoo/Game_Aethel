using CodeBase.Enemies;
using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(EnemyHealth), typeof(EnemyAnimator), typeof(Follow))]
    public class EnemyDeath : MonoBehaviour
    {
        public EnemyHealth Health;
        public EnemyAnimator Animator;
        public Follow follow;

        public GameObject DeathFx;

        // Subject — это "излучатель" события в UniRx
        private readonly Subject<Unit> _deathSubject = new Subject<Unit>();
        public IObservable<Unit> OnDeath => _deathSubject;

        public event Action Happened;

        private void Start()
        {
            Health.HealthChanged += HealthChanged;
        }

        private void OnDestroy()
        { 
            Health.HealthChanged -= HealthChanged;
            _deathSubject.Dispose();
        }

        private void HealthChanged()
        {
            if (Health.Current <= 0)
            {
                follow.IsDied = true;
                Die();
            }
        }

        private void Die()
        {
            Health.HealthChanged -= HealthChanged;

            Animator.PlayDeath();
            SpawnDeathFx();
            StartCoroutine(DestroyTimer());

            Happened?.Invoke();

            _deathSubject.OnNext(Unit.Default); // Рассылаем сигнал смерти
            _deathSubject.OnCompleted();        // Закрываем поток
        }

        private void SpawnDeathFx() =>
            Instantiate(DeathFx, transform.position, Quaternion.identity);

        private IEnumerator DestroyTimer()
        {
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }
    }
}
