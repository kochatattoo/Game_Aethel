using UnityEngine;
using UniRx;
using System;
using CodeBase.Components;

namespace CodeBase.Enemies
{
    public class EnemyVision : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Transform _headTransform;
        [SerializeField]
        private Follow _follow;
        [SerializeField]
        private EnemyDeath _enemyDeath;

        [Header("Vision Settings")]
        [SerializeField, Tooltip("Настройки сенсора")]
        private VisionSensor _sensor;
        [SerializeField]
        private float _viewDistance = 10f;
        [SerializeField]
        private LayerMask _playerMask;

        [Header("Optimization")]
        [SerializeField, Tooltip("Расстояние, после которого логика спит")] 
        private float _sleepDistance = 50f;
        [SerializeField]
        private float _cooldown = 2f;

        private bool _hasTarget;
        private Transform _heroTransform;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly SerialDisposable _cooldownSubscription = new SerialDisposable();

        /// <summary>
        /// Инициализация через Фабрику. 
        /// </summary>
        public void Construct(Transform transform)
        {
            _heroTransform = transform;

            _disposables.Clear();
            _cooldownSubscription.Disposable = Disposable.Empty;
            _disposables.Add(_cooldownSubscription);

            if (_follow != null) 
                _follow.enabled = false;

            if (_headTransform == null)
            {
                Debug.LogError($"[EnemyVision] на {gameObject.name} не назначен HeadTransform (пустышка глаз)!");
                return;
            }

            if (_enemyDeath != null)
            {
                _enemyDeath.OnDeath
                    .Subscribe(_ => HandleDeath())
                    .AddTo(_disposables);
            }

            float interval = 0.5f;
            int uniqueId = Mathf.Abs(gameObject.GetInstanceID());
            float initialDelay = (uniqueId % 10) * (interval / 10f);

           Observable.Timer(TimeSpan.FromSeconds(initialDelay))
                .ContinueWith(Observable.Interval(TimeSpan.FromSeconds(interval)))
                .Subscribe(_ => Tick())
                .AddTo(_disposables);
        }

        private void Tick()
        {
            if (_heroTransform == null)
                return;

            float distSq = (_heroTransform.position - transform.position).sqrMagnitude;
            if (distSq > _sleepDistance * _sleepDistance)
            {
                if (_hasTarget) 
                    OnTargetLost();
                return;
            }

            int visibleCount = _sensor.Scan(_headTransform, _viewDistance, _playerMask, out _);
            bool isSpotted = visibleCount > 0;

            if (isSpotted)
            {
                if (!_hasTarget) 
                    OnTargetSpotted();
            }
            else
            {
                if (_hasTarget) 
                    OnTargetLost();
            }
        }

        private void OnTargetSpotted()
        {
            _hasTarget = true;

            _cooldownSubscription.Disposable = Disposable.Empty;

            if (_follow != null) 
                _follow.enabled = true;
        }

        private void OnTargetLost()
        {
            _hasTarget = false;

            _cooldownSubscription.Disposable = Observable.Timer(TimeSpan.FromSeconds(_cooldown))
                .Subscribe(_ =>
                {
                    if (_follow != null) _follow.enabled = false;
                    Debug.Log("<color=orange>[AGGRO]</color> Цель потеряна. Возврат в режим ожидания.");
                })
                .AddTo(_disposables);
        }

        private void HandleDeath()
        {
            _disposables.Clear();

            if (_follow != null)
                _follow.enabled = false;

            Debug.Log($"[Vision] {gameObject.name} мертв, зрение отключено.");
        }

        private void OnDestroy() => _disposables.Dispose();

        private void OnDrawGizmosSelected()
        {
            if (_headTransform == null) return;

            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, _sleepDistance);

            PhysicsDebug.DrawViewSector(
                _headTransform,
                _sensor.HorizontalAngle, 
                _sensor.VerticalAngle,  
                _viewDistance,
                Color.cyan
            );

            if (Application.isPlaying && _hasTarget && _heroTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_headTransform.position, _heroTransform.position);

                PhysicsDebug.DrawDebug(_heroTransform.position, 0.5f, 0.1f);
            }
        }
    }
}
