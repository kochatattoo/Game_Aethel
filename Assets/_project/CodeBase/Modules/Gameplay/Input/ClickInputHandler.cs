using System;
using System.Collections.Generic;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.Infrastructure.Services
{
    public class ClickInputHandler : IDisposable, IClickListener
    {
        private readonly IInputService _inputService;
        private readonly Blackboard _blackboard;
        private readonly ClickTargetResolver _targetResolver = new();
        private readonly List<RaycastResult> _uiRaycastResults = new();
        private PointerEventData _uiPointerEventData;
        private bool _subscribed;

        public event Action OnProcessed;

        public ClickInputHandler(IInputService inputService, Blackboard blackboard)
        {
            _inputService = inputService;
            _blackboard = blackboard;
        }

        public void Initialize()
        {
            if (_subscribed) return;
            _inputService.Click += OnClick;
            _subscribed = true;
        }

        public void Dispose()
        {
            if (!_subscribed) return;
            _inputService.Click -= OnClick;
            _subscribed = false;
        }

        private void OnClick(Vector3 screenPosition)
        {
            if (CharacterIntegration.MenuInputScope.IsBlocked || IsPointerOverUI(screenPosition)) return;
            OnProcessed?.Invoke();

            var currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (_blackboard.TryGetValue(currentTarget, out TargetData _)) _blackboard.Remove(currentTarget);
            _blackboard.SetValue(currentTarget, _targetResolver.Resolve(screenPosition));
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
