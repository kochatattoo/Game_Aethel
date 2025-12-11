using CodeBase.InputActions;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace CodeBase.Infrastructure.Services
{
    public class NewInputService : IInputService, IInitializable, IDisposable
    {
        private readonly PlayerInputAction _actions;
        private Vector2 _move;

        public Vector2 Axis => _move;

        public NewInputService()
        {
            _actions = new PlayerInputAction();
        }

        public void Initialize()
        {
            Debug.Log("initialize Input Service");

            // Включаем экшен сет и подписываемся
            _actions.Player.Enable();

            _actions.Player.Move.performed += OnMove;
            _actions.Player.Move.canceled += OnMove;

            // Выбираем схему на старте
            ChoiseInpuDevice();

            Debug.Log("Device is " + _actions.devices);
        }

        private void ChoiseInpuDevice()
        {
            if (Application.isMobilePlatform)
                _actions.devices = new InputDevice[] { Touchscreen.current };
            else
                _actions.devices = new InputDevice[] { Keyboard.current, Mouse.current };
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _move = ctx.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            // Отписываемся и выключаем
            _actions.Player.Move.performed -= OnMove;
            _actions.Player.Move.canceled -= OnMove;

            _actions.Player.Disable();
            _actions.Dispose();
        }
    }
}
