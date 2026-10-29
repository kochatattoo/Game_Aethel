using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;

namespace Assets._project.CodeBase.Infrastructure.Services.Input
{
    public class InputHandlerService : IInputHandlerService
    {
        private readonly IInputService _inputService;
        private readonly Blackboard _blackboard;
        private readonly ClickInputHandler _clickInputHandler;

        public ClickInputHandler ClickInputHandler => _clickInputHandler;

        public InputHandlerService(IInputService inputService, IBlackboardService blackboardService)
        {
            _inputService = inputService;
            _blackboard = blackboardService.Blackboard;
            _clickInputHandler = new ClickInputHandler(inputService, blackboardService.Blackboard);
        }
    }
}
