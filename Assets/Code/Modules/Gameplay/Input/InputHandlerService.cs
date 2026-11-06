using CodeBase.Infrastructure.Services;
using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace Assets._project.CodeBase.Infrastructure.Services.Input
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
