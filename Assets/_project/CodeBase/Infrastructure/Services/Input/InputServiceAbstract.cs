using CodeBase.InputActions;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace CodeBase.Infrastructure.Services
{
    public abstract class InputServiceAbstract: IInputService, IInitializable, IDisposable
    {
        protected PlayerInputAction _actions;
        protected Vector2 _move;

        public event Action Attack;
        public event Action<Vector3> Click;

        public Vector2 Axis => _move;

        public void Initialize() => Subscribe();

        public void Dispose() => Unsubscribe();

        public abstract void Subscribe();

        public abstract void Unsubscribe();

        protected void AttackAction(InputAction.CallbackContext context)
        {
            Attack?.Invoke();
        }

        protected void ClickAction(InputAction.CallbackContext context)
        {
            Debug.Log("Click action");
            if (!context.performed) return;
           
           // Vector2 pointerPos = context.ReadValue<Vector2>();
           // Vector2 pointerPos = _actions.Player.Point.ReadValue<Vector2>();

            Vector2 pointerPos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
            Click?.Invoke(pointerPos);
        }

        protected void ChoiseInpuDevice()
        {
            if (Application.isMobilePlatform)
                _actions.devices = new InputDevice[] { Touchscreen.current };
            else
                _actions.devices = new InputDevice[] { Keyboard.current, Mouse.current };
        }
    }
}
