using CodeBase.Hero;
using UnityEngine;
using UniRx;
using System;

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
        [SerializeField]
        private float _viewDistance = 10f;
        [SerializeField, Range(0, 180), Tooltip("Горизонтальный угол обзора (ширина)")]
        private float _viewAngle = 150f;
        [SerializeField, Range(0, 180), Tooltip("Вертикальный угол обзора (высота)")] 
        private float _viewHeightAngle = 45f;

        [Header("Optimization")]
        [SerializeField, Tooltip("Расстояние, после которого логика спит")] 
        private float _sleepDistance = 50f;

        [SerializeField]
        private LayerMask _playerMask;
        [SerializeField]
        private LayerMask _obstacleMask;
        [SerializeField]
        private float _cooldown = 2f;

        private readonly Collider[] _overlapBuffer = new Collider[1];
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

            // Очистка старых подписок (важно для пулинга)
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
                .Subscribe(_ => CheckVision())
                .AddTo(_disposables);
        }

      
        private void OnDestroy() => _disposables.Dispose();

        private void CheckVision()
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

            int count = Physics.OverlapSphereNonAlloc(_headTransform.position, _viewDistance, _overlapBuffer, _playerMask);
            bool targetSpottedThisFrame = false;

            if (count > 0 && _overlapBuffer[0] != null)
            {
                if (_overlapBuffer[0].TryGetComponent(out IVisibilityPointsProvider visibilityProvider))
                {
                    if (IsAnyPointVisible(visibilityProvider))
                    {
                        targetSpottedThisFrame = true;
                    }
                }
            }

            if (targetSpottedThisFrame)
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

        private bool IsAnyPointVisible(IVisibilityPointsProvider provider)
        {
            foreach (var point in provider.Points)
            {
                if (point == null) 
                    continue;

                Vector3 directionToPoint = point.position - _headTransform.position;
                float distanceToPoint = directionToPoint.magnitude;

                Vector3 localDir = _headTransform.InverseTransformDirection(directionToPoint);

                float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                if (Mathf.Abs(horizontalAngle) > _viewAngle / 2f) 
                    continue;

                float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
                if (Mathf.Abs(verticalAngle) > _viewHeightAngle / 2f) 
                    continue;

                if (!Physics.Raycast(_headTransform.position, directionToPoint.normalized, out RaycastHit hit, distanceToPoint, _obstacleMask))
                {
                    Debug.DrawLine(_headTransform.position, point.position, Color.green, 0.1f);
                    return true;
                }
                else
                {
                    Debug.DrawLine(_headTransform.position, hit.point, Color.red, 0.1f);
                }
            }
            return false;
        }

        private void OnTargetSpotted()
        {
            _hasTarget = true;

            // Прерываем таймер потери цели, если он шел
            _cooldownSubscription.Disposable = Disposable.Empty;

            if (_follow != null) 
                _follow.enabled = true;
        }

        private void OnTargetLost()
        {
            _hasTarget = false;

            // Запускаем таймер через SerialDisposable (автоматически отменит предыдущий, если был)
            _cooldownSubscription.Disposable = Observable.Timer(TimeSpan.FromSeconds(_cooldown))
                .Subscribe(_ =>
                {
                    if (_follow != null) _follow.enabled = false;
                    Debug.Log("<color=orange>[AGGRO]</color> Цель потеряна. Возврат в режим ожидания.");
                })
                .AddTo(_disposables); // Защита: очистится при смерти врага
        }

        private void HandleDeath()
        {
            // Очищаем ВСЕ подписки (зрение и таймеры Cooldown сразу остановятся)
            _disposables.Clear();

            if (_follow != null)
                _follow.enabled = false;

            Debug.Log($"[Vision] {gameObject.name} мертв, зрение отключено.");
        }


        private void OnDrawGizmosSelected()
        {
            if (_headTransform == null) 
                return;

            PhysicsDebug.DrawViewSector(_headTransform, _viewAngle, _viewHeightAngle, _viewDistance, Color.cyan);

            if (Application.isPlaying && _hasTarget && _overlapBuffer[0] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_headTransform.position, _overlapBuffer[0].transform.position);
            }
        }
    }
}
