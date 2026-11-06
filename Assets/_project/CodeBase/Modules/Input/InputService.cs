using System;
using CodeBase.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace CodeBase.Infrastructure.Services
{
    public sealed class InputService : IInputService, IInitializable, IDisposable
    {
        private readonly PlayerInputAction _actions = new();
        private bool _initialized;
        private bool _disposed;

        public event Action<Vector3> Click;

        public void Initialize()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(InputService));
            if (_initialized) return;
            SelectInputDevices();
            _actions.Player.Click.performed += OnClick;
            _actions.Player.Enable();
            _initialized = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_initialized) _actions.Player.Click.performed -= OnClick;
            _actions.Player.Disable();
            _actions.Dispose();
            _disposed = true;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            var pointerPosition = _actions.Player.Point.ReadValue<Vector2>();
            Click?.Invoke(pointerPosition);
        }

        private void SelectInputDevices()
        {
            if (Application.isMobilePlatform)
            {
                if (Touchscreen.current != null) _actions.devices = new InputDevice[] { Touchscreen.current };
                return;
            }
            if (Keyboard.current != null && Mouse.current != null)
                _actions.devices = new InputDevice[] { Keyboard.current, Mouse.current };
        }
    }
}
