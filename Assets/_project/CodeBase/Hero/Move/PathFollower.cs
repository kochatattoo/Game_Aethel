using CodeBase.Infrastructure.Services;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Hero
{
    public class PathFollower : IDisposable
    {
        public float maxDistance = 100f;

        private readonly IInputService _inputService;
        private readonly Camera _camera;          // Камера для ray‑casting

        private Vector3 _targetPos;
        private bool _hasTarget;
        private static int _layerMask;

        public event Action<Vector3> OnNewDestination;   // <-- событие

        public PathFollower(IInputService inputService, Camera camera = null)
        {
            _inputService = inputService;
            _camera = camera ?? Camera.main;
            _layerMask = 1 << LayerMask.NameToLayer("Ground");
        }

        public void Initialize()
        {
            Subscribe();
        }

        public void Update()
        {
            // Самый простой вариант – отслеживаем только флаг
            if (_hasTarget && OnNewDestination != null)
            {
                // Когда NavMeshAgent сам дойдёт, он отменит задачу в HeroMove
                _hasTarget = false;
            }
        }

        public void Dispose()
        {
            UnSubscribe();
        }

        private void Subscribe() => _inputService.Click += OnClicked;
        private void UnSubscribe() => _inputService.Click -= OnClicked;

        private void OnClicked(Vector3 screenPos)
        {

            Debug.Log("Вызов события клика");

            Ray ray = _camera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _layerMask))
            {
                // Проверяем доступность точки на NavMesh
                if (!NavMesh.SamplePosition(hit.point, out var navHit, 1f, NavMesh.AllAreas))
                    return;

                Vector3 pointOnNavMesh = navHit.position;
                _targetPos = pointOnNavMesh;
                _hasTarget = true;

                Debug.Log("Позиция на экране " + pointOnNavMesh);

                // Уведомляем HeroMove
                OnNewDestination?.Invoke(pointOnNavMesh);

                Debug.Log("Вызов события изменения дистанции");
            }
        }
    }
}
