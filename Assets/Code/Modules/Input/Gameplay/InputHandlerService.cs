using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace CodeBase.Infrastructure.Services.Input
{
    public class InputHandlerService : IInputHandlerService
    {
        public ClickInputHandler ClickInputHandler { get; }

        public InputHandlerService(IInputService inputService, IBlackboardService blackboardService)
        {
            ClickInputHandler = new ClickInputHandler(inputService, blackboardService.Blackboard);
        }
    }
}
