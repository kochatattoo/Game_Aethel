namespace CodeBase.Infrastructure.State
{
    public class BootstrapState : IState
    {
        private const string Bootstrap = "Bootstrap";
        private readonly IGameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(IGameStateMachine stateMachine, SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
        }

        public void Enter()
        {
            _sceneLoader.Load(Bootstrap, EnterLoadLevel);
        }
        public void Exit()
        {

        }

        private void EnterLoadLevel() =>
            _stateMachine.Enter<LoadLevelState, string>("Level_1");

    }
}
