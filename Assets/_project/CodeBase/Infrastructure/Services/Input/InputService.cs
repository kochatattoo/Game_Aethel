using CodeBase.InputActions;
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
            ChoiseInpuDevice();
        }

        public override void Unsubscribe()
        {

        }

        private void OnClick(InputAction.CallbackContext context)
        {
            ClickAction(context);
        }
    }
}
