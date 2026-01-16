using CodeBase.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Infrastructure.Services
{
    public class InputService : InputServiceAbstract
    {
        public InputService() 
        {
            _actions = new PlayerInputAction();
        }

        public override void Subscribe()
        {
            _actions.Player.Enable();

            _actions.Player.Click.performed += OnClick;

            ChoiseInpuDevice();
        }

        public override void Unsubscribe()
        {
            _actions.Player.Click.performed -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            ClickAction(context);
        }
    }
}
