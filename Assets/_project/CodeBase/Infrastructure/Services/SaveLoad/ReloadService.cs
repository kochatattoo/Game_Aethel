using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class ReloadService : IReloadService
    {
        private readonly GameStateMachine _gameStateMachine;

        public ReloadService(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Reload()
        {
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}
