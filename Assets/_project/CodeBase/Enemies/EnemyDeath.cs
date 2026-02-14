using System;
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
        private bool _isDead;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            // Подписываемся на здоровье: фильтруем значения <= 0 и берем только ПЕРВОЕ срабатывание
            Health.Current
                .Where(h => h <= 0)
                .First()
                .Subscribe(_ => Die())
                .AddTo(_disposables);
        }

        private void Die()
        {
            if (follow != null) 
                follow.IsDied = true;

            Animator.PlayDeath();
            SpawnDeathFx();

            Happened?.Invoke();
            _deathSubject.OnNext(Unit.Default);
            _deathSubject.OnCompleted();

            Observable.Timer(TimeSpan.FromSeconds(3f))
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(_disposables);
        }

        private void SpawnDeathFx() =>
            Instantiate(DeathFx, transform.position, Quaternion.identity);

        private void OnDestroy()
        {
            _deathSubject.Dispose();
            _disposables.Dispose();
        }
    }
}
