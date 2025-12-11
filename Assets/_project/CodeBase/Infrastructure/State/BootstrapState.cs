using CodeBase.Infrastructure.Services.StaticData;

namespace CodeBase.Infrastructure.State
{
    public class BootstrapState : IState
    {
        private const string Bootstrap = "Bootstrap";
        private readonly IGameStateMachine _stateMachine;
        private readonly IStaticDataService _staticDataService;
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(IGameStateMachine stateMachine, IStaticDataService staticDataService, SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _staticDataService = staticDataService;

            _staticDataService.Load();
        }

        public void Enter()
        {
            _sceneLoader.Load(Bootstrap, EnterLoadLevel);
        }
        public void Exit()
        {

        }

        private void EnterLoadLevel() =>
            _stateMachine.Enter<LoadProgressState>();

    }
}
