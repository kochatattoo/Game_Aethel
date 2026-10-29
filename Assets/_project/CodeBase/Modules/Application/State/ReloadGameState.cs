using CodeBase.CharacterIntegration;

namespace CodeBase.Infrastructure.State
{
    public class ReloadGameState : IState
    {
        private const string Reload = "Reload";
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly ILoadingScreen _curtain;

        public ReloadGameState(IGameStateMachine gameStateMachine, SceneLoader sceneLoader, ILoadingScreen curtain)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
        }

        public void Enter()
        {
            _curtain.Show();
            _sceneLoader.Load(Reload, EnterLoadLevel);
        }

        public void Exit()
        {

        }

        private void EnterLoadLevel()
        {
            _gameStateMachine.Enter<LoadProgressState>();
        }
    }
}
