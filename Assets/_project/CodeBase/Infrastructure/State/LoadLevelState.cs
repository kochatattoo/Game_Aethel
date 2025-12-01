using CodeBase.Logic;

namespace CodeBase.Infrastructure.State
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly LoadingCurtain _curtain;
        private readonly SceneLoader _sceneLoader;

        public LoadLevelState (IGameStateMachine stateMachine, LoadingCurtain curtain, SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _curtain = curtain;
            _sceneLoader = sceneLoader;
        }

        public void Enter(string sceneName)
        {
            _curtain.Show();
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit() { }

        private void OnLoaded()
        {
            _curtain.Hide();
            _stateMachine.Enter<GameLoopState>();
        }
    }
}
