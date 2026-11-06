using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace CodeBase.MotionTest
{
    public sealed class MotionPointerInput : IDisposable
    {
        private InputActionAsset _actions;
        private readonly Func<bool> _pointerFilter;
        private readonly Func<bool> _keyboardFilter;

        public InputActionMap Actions { get; private set; }
        public bool PointerAllowed => _pointerFilter?.Invoke() ?? true;
        public bool KeyboardAllowed => _keyboardFilter?.Invoke() ?? true;
        public Vector2 PointerPosition => Actions["Point"].ReadValue<Vector2>();
        public bool MovePressed => Actions["Move"].WasPressedThisFrame();
        public bool Orbiting => Actions["Orbit"].IsPressed();

        public MotionPointerInput(InputActionAsset actions, Func<bool> pointerFilter, Func<bool> keyboardFilter)
        {
            _pointerFilter = pointerFilter;
            _keyboardFilter = keyboardFilter;
            _actions = Object.Instantiate(actions);
            Actions = _actions.FindActionMap("Motion", true);
            Actions.Enable();
        }

        public bool TryRaycast(Camera camera, out RaycastHit hit)
        {
            var cursor = PointerPosition;
            if (cursor.x < 0 || cursor.y < 0 || cursor.x >= Screen.width || cursor.y >= Screen.height)
            {
                hit = default;
                return false;
            }

            return Physics.Raycast(camera.ScreenPointToRay(cursor), out hit, 300, ~(1 << 2),
                QueryTriggerInteraction.Collide);
        }

        public void Dispose()
        {
            if (_actions == null) return;
            _actions.Disable();
            Object.Destroy(_actions);
            _actions = null;
            Actions = null;
        }
    }
}
