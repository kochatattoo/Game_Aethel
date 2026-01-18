using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.Infrastructure.Services
{
    public class ClickInputHandler : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly Blackboard _blackboard;
        private readonly Camera _camera;

        private float maxDistance = 100f;

        private static int _layerMask;

        public event Action OnProcessed;

        public ClickInputHandler(IInputService inputService, Blackboard blackboard, Camera camera)
        {
            _inputService = inputService;
            _blackboard = blackboard;
            _camera = camera;
        }

        public void Initialize()
        {
            Subscribe();
        }

        public void Dispose()
        {
           UnSubscribe();
        }

        private void Subscribe()
        {
            _inputService.Click += OnClick;
        }

        private void UnSubscribe()
        {
            _inputService.Click -= OnClick;
        }

        private void OnClick(Vector3 vector)
        {
            BlackboardKey position = new ("Position");
            _blackboard.SetValue(position, vector);

            Debug.Log("Вызов события клика");

            // TODO: Сделать логику передачи ID устройства (мобильное : 0 / ПК : -1)
            if (EventSystem.current.IsPointerOverGameObject(pointerId: 0))
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(vector);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _layerMask))
            {
                // Обрабатываем полученые данные, что бы записать их в Blackboard для нашего
                OnProcessed?.Invoke();
            }
        }
    }
}
