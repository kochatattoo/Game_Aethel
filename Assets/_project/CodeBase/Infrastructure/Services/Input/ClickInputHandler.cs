using CodeBase.Hero;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.Infrastructure.Services
{
    public class ClickInputHandler : IDisposable, IClickListener
    {
        private const float MAX_DIST = 200f;

        private readonly IInputService _inputService;
        private readonly Blackboard _blackboard;
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();

        private PointerEventData _uiPointerEventData;

        private static readonly int LayerGround = 1 << LayerMask.NameToLayer("Ground");
        private static readonly int LayerUnits = 1 << LayerMask.NameToLayer("Units");
        private static readonly int LayerInteract = 1 << LayerMask.NameToLayer("Interactable");
        private static readonly int LayerDefault = 1 << LayerMask.NameToLayer("Default");

        private static readonly int _layerMask =
           LayerGround
         | LayerUnits
         | LayerInteract
         | LayerDefault;

        public event Action OnProcessed;

        public ClickInputHandler(
            IInputService inputService, 
            Blackboard blackboard)
        {
            _inputService = inputService;
            _blackboard = blackboard;
        }

        public void Initialize() => Subscribe();

        public void Dispose() => UnSubscribe();

        private void Subscribe() => _inputService.Click += OnClick;

        private void UnSubscribe() => _inputService.Click -= OnClick;

        private void OnClick(Vector3 screenPosition)
        {
            // TODO: Сделать логику передачи ID устройства (мобильное : 0 / ПК : -1)
            //if (EventSystem.current.IsPointerOverGameObject(pointerId: -1))
            //    return;

            if (IsPointerOverUI(screenPosition))
                return;

            OnProcessed?.Invoke();

            //Debug.Log("Вызов события клика");

            BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (_blackboard.TryGetValue(currentTarget, out TargetData data))
            {
                _blackboard.Remove(currentTarget);
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, MAX_DIST, _layerMask))
            {
                _blackboard.SetValue(currentTarget, new TargetData (TargetType.None, Vector3.zero));
                return;
            }

            TargetData targetData;
            GameObject targetGameObject = hit.collider.gameObject;

            if ((LayerUnits & (1 << targetGameObject.layer)) != 0)
                targetData = new TargetData(TargetType.Attack, hit.point, targetGameObject);
            else if ((LayerInteract & (1 << targetGameObject.layer)) != 0)
                targetData = new TargetData(TargetType.Interact, hit.point, targetGameObject);
            else // ground
                targetData = new TargetData(TargetType.Move, hit.point, null);

            Debug.Log(targetData.Type);

            _blackboard.SetValue(currentTarget, targetData);
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;

            _uiPointerEventData ??= new PointerEventData(EventSystem.current);
            _uiPointerEventData.position = screenPosition;
            _uiRaycastResults.Clear();


            EventSystem.current.RaycastAll(_uiPointerEventData, _uiRaycastResults);


            return _uiRaycastResults.Count > 0;
        }
    }
}
