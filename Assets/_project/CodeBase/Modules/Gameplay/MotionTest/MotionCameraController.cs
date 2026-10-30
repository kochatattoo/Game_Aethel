using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.MotionTest
{
    public sealed class MotionCameraController : MonoBehaviour
    {
        public CinemachineCamera VirtualCamera;
        public CinemachineOrbitalFollow Orbit;
        public Transform Pivot;
        private Transform _hero;
        private MotionTestController _host;
        private InputActionMap _input;
        public float Yaw => Orbit.HorizontalAxis.Value;
        public float Pitch => Orbit.VerticalAxis.Value;
        public float Zoom => VirtualCamera.Lens.OrthographicSize;
        public bool Orbiting => _input != null && _input["Orbit"].IsPressed();
        public void Bind(MotionTestController host, Transform hero, InputActionMap input) { _host = host; _hero = hero; _input = input; Focus(); }
        public void Focus()
        {
            Orbit.HorizontalAxis.Value = 45; Orbit.VerticalAxis.Value = 45;
            VirtualCamera.Lens.OrthographicSize = 14;
            VirtualCamera.PreviousStateIsValid = false;
        }
        private void Update()
        {
            if (_hero == null || _input == null) return;
            Pivot.position = _hero.position + Vector3.up * 1.8f;
            if (_host.KeyboardInputAllowed)
            {
                if (_input["YawLeft"].WasPressedThisFrame()) Orbit.HorizontalAxis.Value -= 45;
                if (_input["YawRight"].WasPressedThisFrame()) Orbit.HorizontalAxis.Value += 45;
                if (_input["Focus"].WasPressedThisFrame()) Focus();
            }
            if (!_host.PointerInputAllowed) return;
            var mouse = _input["Point"].ReadValue<Vector2>();
            if (mouse.x < 0 || mouse.y < 0 || mouse.x >= Screen.width || mouse.y >= Screen.height) return;
            if (Orbiting)
            {
                var look = _input["Look"].ReadValue<Vector2>();
                Orbit.HorizontalAxis.Value = Mathf.Repeat(Orbit.HorizontalAxis.Value + look.x * .2f + 180, 360) - 180;
                Orbit.VerticalAxis.Value = Mathf.Clamp(Orbit.VerticalAxis.Value - look.y * .15f, 30, 65);
            }
            var scroll = _input["Zoom"].ReadValue<float>();
            // Input System can report either normalized notches or legacy 120-unit wheel deltas.
            var notches = Mathf.Abs(scroll) > 10 ? scroll / 120 : scroll;
            VirtualCamera.Lens.OrthographicSize = Mathf.Clamp(Zoom - notches * .8f, 7, 35);
        }
    }
}
